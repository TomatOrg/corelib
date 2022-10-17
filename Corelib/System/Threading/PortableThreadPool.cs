// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Threading
{
    /// <summary>
    /// A thread-pool run and managed on the CLR.
    /// </summary>
    internal sealed partial class PortableThreadPool
    {
        private const int ThreadPoolThreadTimeoutMs = 20 * 1000; // If you change this make sure to change the timeout times in the tests.
        private const int SmallStackSizeBytes = 256 * 1024;

        private const short MaxPossibleThreadCount = short.MaxValue;
        private const short DefaultMaxWorkerThreadCount = MaxPossibleThreadCount;

        private const int CpuUtilizationHigh = 95;
        private const int CpuUtilizationLow = 80;

        [ThreadStatic]
        private static object? t_completionCountObject;

#pragma warning disable IDE1006 // Naming Styles
        // The singleton must be initialized after the static variables above, as the constructor may be dependent on them.
        // SOS's ThreadPool command depends on this name.
        public static readonly PortableThreadPool ThreadPoolInstance = new PortableThreadPool();
#pragma warning restore IDE1006 // Naming Styles

        private int _cpuUtilization; // SOS's ThreadPool command depends on this name
        private short _minThreads;
        private short _maxThreads;

        [StructLayout(LayoutKind.Explicit, Size = Internal.PaddingHelpers.CACHE_LINE_SIZE * 6)]
        private struct CacheLineSeparated
        {
            [FieldOffset(Internal.PaddingHelpers.CACHE_LINE_SIZE * 1)]
            public ThreadCounts counts; // SOS's ThreadPool command depends on this name

            [FieldOffset(Internal.PaddingHelpers.CACHE_LINE_SIZE * 2)]
            public int lastDequeueTime;

            [FieldOffset(Internal.PaddingHelpers.CACHE_LINE_SIZE * 3)]
            public int priorCompletionCount;
            [FieldOffset(Internal.PaddingHelpers.CACHE_LINE_SIZE * 3 + sizeof(int))]
            public int priorCompletedWorkRequestsTime;
            [FieldOffset(Internal.PaddingHelpers.CACHE_LINE_SIZE * 3 + sizeof(int) * 2)]
            public int nextCompletedWorkRequestsTime;

            [FieldOffset(Internal.PaddingHelpers.CACHE_LINE_SIZE * 4)]
            public volatile int numRequestedWorkers;
            [FieldOffset(Internal.PaddingHelpers.CACHE_LINE_SIZE * 4 + sizeof(int))]
            public int gateThreadRunningState;
        }

        private long _currentSampleStartTime;
        private readonly ThreadInt64PersistentCounter _completionCounter = new ThreadInt64PersistentCounter();
        private int _threadAdjustmentIntervalMs;

        private short _numBlockedThreads;
        private short _numThreadsAddedDueToBlocking;
        private PendingBlockingAdjustment _pendingBlockingAdjustment;

        private long _memoryUsageBytes;
        private long _memoryLimitBytes;

        private readonly LowLevelLock _threadAdjustmentLock = new LowLevelLock();

        private CacheLineSeparated _separated; // SOS's ThreadPool command depends on this name

        private PortableThreadPool()
        {
            _minThreads = (short)Environment.ProcessorCount;
            if (_minThreads > MaxPossibleThreadCount)
            {
                _minThreads = MaxPossibleThreadCount;
            }

            _maxThreads = DefaultMaxWorkerThreadCount;
            if (_maxThreads > MaxPossibleThreadCount)
            {
                _maxThreads = MaxPossibleThreadCount;
            }
            else if (_maxThreads < _minThreads)
            {
                _maxThreads = _minThreads;
            }

            _separated.counts.NumThreadsGoal = _minThreads;
        }

        public bool SetMinThreads(int workerThreads, int ioCompletionThreads)
        {
            if (workerThreads < 0 || ioCompletionThreads < 0)
            {
                return false;
            }

            bool addWorker = false;
            bool wakeGateThread = false;

            _threadAdjustmentLock.Acquire();
            try
            {
                if (workerThreads > _maxThreads || !ThreadPool.CanSetMinIOCompletionThreads(ioCompletionThreads))
                {
                    return false;
                }

                ThreadPool.SetMinIOCompletionThreads(ioCompletionThreads);

                short newMinThreads = (short)Math.Max(1, Math.Min(workerThreads, MaxPossibleThreadCount));
                _minThreads = newMinThreads;
                if (_numBlockedThreads > 0)
                {
                    // Blocking adjustment will adjust the goal according to its heuristics
                    if (_pendingBlockingAdjustment != PendingBlockingAdjustment.Immediately)
                    {
                        _pendingBlockingAdjustment = PendingBlockingAdjustment.Immediately;
                        wakeGateThread = true;
                    }
                }
                else if (_separated.counts.NumThreadsGoal < newMinThreads)
                {
                    _separated.counts.InterlockedSetNumThreadsGoal(newMinThreads);
                    if (_separated.numRequestedWorkers > 0)
                    {
                        addWorker = true;
                    }
                }
            }
            finally
            {
                _threadAdjustmentLock.Release();
            }

            if (addWorker)
            {
                WorkerThread.MaybeAddWorkingWorker(this);
            }
            else if (wakeGateThread)
            {
                GateThread.Wake(this);
            }
            return true;
        }

        public int GetMinThreads() => Volatile.Read(ref _minThreads);

        public bool SetMaxThreads(int workerThreads, int ioCompletionThreads)
        {
            if (workerThreads <= 0 || ioCompletionThreads <= 0)
            {
                return false;
            }

            _threadAdjustmentLock.Acquire();
            try
            {
                if (workerThreads < _minThreads || !ThreadPool.CanSetMaxIOCompletionThreads(ioCompletionThreads))
                {
                    return false;
                }

                ThreadPool.SetMaxIOCompletionThreads(ioCompletionThreads);

                short newMaxThreads = (short)Math.Min(workerThreads, MaxPossibleThreadCount);
                _maxThreads = newMaxThreads;
                if (_separated.counts.NumThreadsGoal > newMaxThreads)
                {
                    _separated.counts.InterlockedSetNumThreadsGoal(newMaxThreads);
                }
                return true;
            }
            finally
            {
                _threadAdjustmentLock.Release();
            }
        }

        public int GetMaxThreads() => Volatile.Read(ref _maxThreads);

        public int GetAvailableThreads()
        {
            ThreadCounts counts = _separated.counts.VolatileRead();
            int count = _maxThreads - counts.NumProcessingWork;
            if (count < 0)
            {
                return 0;
            }
            return count;
        }

        public int ThreadCount => _separated.counts.VolatileRead().NumExistingThreads;
        public long CompletedWorkItemCount => _completionCounter.Count;

        public object GetOrCreateThreadLocalCompletionCountObject() =>
            t_completionCountObject ?? CreateThreadLocalCompletionCountObject();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private object CreateThreadLocalCompletionCountObject()
        {
            Debug.Assert(t_completionCountObject == null);

            object threadLocalCompletionCountObject = _completionCounter.CreateThreadLocalCountObject();
            t_completionCountObject = threadLocalCompletionCountObject;
            return threadLocalCompletionCountObject;
        }

        private void NotifyWorkItemProgress(object threadLocalCompletionCountObject, int currentTimeMs)
        {
            ThreadInt64PersistentCounter.Increment(threadLocalCompletionCountObject);
            _separated.lastDequeueTime = currentTimeMs;
        }

        internal void NotifyWorkItemProgress() =>
            NotifyWorkItemProgress(GetOrCreateThreadLocalCompletionCountObject(), Environment.TickCount);

        internal bool NotifyWorkItemComplete(object? threadLocalCompletionCountObject, int currentTimeMs)
        {
            Debug.Assert(threadLocalCompletionCountObject != null);

            NotifyWorkItemProgress(threadLocalCompletionCountObject!, currentTimeMs);
            return !WorkerThread.ShouldStopProcessingWorkNow(this);
        }

        //
        // This method must only be called if ShouldAdjustMaxWorkersActive has returned true, *and*
        // _hillClimbingThreadAdjustmentLock is held.
        //

        internal void RequestWorker()
        {
            // The order of operations here is important. MaybeAddWorkingWorker() and EnsureRunning() use speculative checks to
            // do their work and the memory barrier from the interlocked operation is necessary in this case for correctness.
            Interlocked.Increment(ref _separated.numRequestedWorkers);
            WorkerThread.MaybeAddWorkingWorker(this);
            GateThread.EnsureRunning(this);
        }

        // private bool OnGen2GCCallback()
        // {
        //     // Gen 2 GCs may be very infrequent in some cases. If it becomes an issue, consider updating the memory usage more
        //     // frequently. The memory usage is only used for fallback purposes in blocking adjustment, so an artifically higher
        //     // memory usage may cause blocking adjustment to fall back to slower adjustments sooner than necessary.
        //     GCMemoryInfo gcMemoryInfo = GC.GetGCMemoryInfo();
        //     _memoryLimitBytes = gcMemoryInfo.HighMemoryLoadThresholdBytes;
        //     _memoryUsageBytes = Math.Min(gcMemoryInfo.MemoryLoadBytes, gcMemoryInfo.HighMemoryLoadThresholdBytes);
        //     return true; // continue receiving gen 2 GC callbacks
        // }
    }
}

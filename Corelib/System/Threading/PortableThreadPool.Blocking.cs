// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace System.Threading
{
    internal sealed partial class PortableThreadPool
    {
        public short MinThreadsGoal
        {
            get
            {
                _threadAdjustmentLock.VerifyIsLocked();
                return Math.Min(_separated.counts.NumThreadsGoal, TargetThreadsGoalForBlockingAdjustment);
            }
        }

        private short TargetThreadsGoalForBlockingAdjustment
        {
            get
            {
                _threadAdjustmentLock.VerifyIsLocked();

                return
                    _numBlockedThreads <= 0
                        ? _minThreads
                        : (short)Math.Min((ushort)(_minThreads + _numBlockedThreads), (ushort)_maxThreads);
            }
        }

        public bool NotifyThreadBlocked()
        {
            if (!Thread.CurrentThread.IsThreadPoolThread)
            {
                return false;
            }

            bool wakeGateThread = false;
            _threadAdjustmentLock.Acquire();
            try
            {
                _numBlockedThreads++;
                Debug.Assert(_numBlockedThreads > 0);

                if (_pendingBlockingAdjustment != PendingBlockingAdjustment.WithDelayIfNecessary &&
                    _separated.counts.NumThreadsGoal < TargetThreadsGoalForBlockingAdjustment)
                {
                    if (_pendingBlockingAdjustment == PendingBlockingAdjustment.None)
                    {
                        wakeGateThread = true;
                    }
                    _pendingBlockingAdjustment = PendingBlockingAdjustment.WithDelayIfNecessary;
                }
            }
            finally
            {
                _threadAdjustmentLock.Release();
            }

            if (wakeGateThread)
            {
                GateThread.Wake(this);
            }
            return true;
        }

        public void NotifyThreadUnblocked()
        {
            Debug.Assert(Thread.CurrentThread.IsThreadPoolThread);

            bool wakeGateThread = false;
            _threadAdjustmentLock.Acquire();
            try
            {
                Debug.Assert(_numBlockedThreads > 0);
                _numBlockedThreads--;

                if (_pendingBlockingAdjustment != PendingBlockingAdjustment.Immediately &&
                    _numThreadsAddedDueToBlocking > 0 &&
                    _separated.counts.NumThreadsGoal > TargetThreadsGoalForBlockingAdjustment)
                {
                    wakeGateThread = true;
                    _pendingBlockingAdjustment = PendingBlockingAdjustment.Immediately;
                }
            }
            finally
            {
                _threadAdjustmentLock.Release();
            }

            if (wakeGateThread)
            {
                GateThread.Wake(this);
            }
        }

        private uint PerformBlockingAdjustment(bool previousDelayElapsed)
        {
            uint nextDelayMs;
            bool addWorker;
            _threadAdjustmentLock.Acquire();
            try
            {
                nextDelayMs = PerformBlockingAdjustment(previousDelayElapsed, out addWorker);
            }
            finally
            {
                _threadAdjustmentLock.Release();
            }

            if (addWorker)
            {
                WorkerThread.MaybeAddWorkingWorker(this);
            }
            return nextDelayMs;
        }

        private uint PerformBlockingAdjustment(bool previousDelayElapsed, out bool addWorker)
        {
            _threadAdjustmentLock.VerifyIsLocked();
            Debug.Assert(_pendingBlockingAdjustment != PendingBlockingAdjustment.None);

            _pendingBlockingAdjustment = PendingBlockingAdjustment.None;
            addWorker = false;

            short targetThreadsGoal = TargetThreadsGoalForBlockingAdjustment;
            ThreadCounts counts = _separated.counts;
            short numThreadsGoal = counts.NumThreadsGoal;
            if (numThreadsGoal == targetThreadsGoal)
            {
                return 0;
            }

            if (numThreadsGoal > targetThreadsGoal)
            {
                // The goal is only decreased by how much it was increased in total due to blocking adjustments. This is to
                // allow blocking adjustments to play well with starvation and hill climbing, either of which may increase the
                // goal independently for other reasons, and blocking adjustments should not undo those changes.
                if (_numThreadsAddedDueToBlocking <= 0)
                {
                    return 0;
                }

                short toSubtract = Math.Min((short)(numThreadsGoal - targetThreadsGoal), _numThreadsAddedDueToBlocking);
                _numThreadsAddedDueToBlocking -= toSubtract;
                numThreadsGoal -= toSubtract;
                _separated.counts.InterlockedSetNumThreadsGoal(numThreadsGoal);
                // HillClimbing.ThreadPoolHillClimber.ForceChange(
                //     numThreadsGoal,
                //     HillClimbing.StateOrTransition.CooperativeBlocking);
                return 0;
            }

            short configuredMaxThreadsWithoutDelay =
                (short)Math.Min((ushort)(_minThreads + BlockingConfig.ThreadsToAddWithoutDelay), (ushort)_maxThreads);

            do
            {
                // Calculate how many threads can be added without a delay. Threads that were already created but may be just
                // waiting for work can be released for work without a delay, but creating a new thread may need a delay.
                short maxThreadsGoalWithoutDelay =
                    Math.Max(configuredMaxThreadsWithoutDelay, Math.Min(counts.NumExistingThreads, _maxThreads));
                short targetThreadsGoalWithoutDelay = Math.Min(targetThreadsGoal, maxThreadsGoalWithoutDelay);
                short newNumThreadsGoal;
                if (numThreadsGoal < targetThreadsGoalWithoutDelay)
                {
                    newNumThreadsGoal = targetThreadsGoalWithoutDelay;
                }
                else if (previousDelayElapsed)
                {
                    newNumThreadsGoal = (short)(numThreadsGoal + 1);
                }
                else
                {
                    // Need to induce a delay before adding a thread
                    break;
                }

                do
                {
                    if (newNumThreadsGoal <= counts.NumExistingThreads)
                    {
                        break;
                    }

                    //
                    // Threads would likely need to be created to compensate for blocking, so check memory usage and limits
                    //

                    long memoryLimitBytes = _memoryLimitBytes;
                    if (memoryLimitBytes <= 0)
                    {
                        break;
                    }

                    // Memory usage is updated after gen 2 GCs, and roughly represents how much physical memory was in use at
                    // the time of the last gen 2 GC. When new threads are also blocking, they may not have used their typical
                    // amount of stack space, and gen 2 GCs may not be happening to update the memory usage. Account for a bit
                    // of extra stack space usage in the future for each thread.
                    long memoryUsageBytes =
                        _memoryUsageBytes +
                        counts.NumExistingThreads * (long)WorkerThread.EstimatedAdditionalStackUsagePerThreadBytes;

                    // The memory limit may already be less than the total amount of physical memory. We are only accounting for
                    // thread pool worker threads above, and after fallback starvation may have to continue creating threads
                    // slowly to prevent a deadlock, so calculate a threshold before falling back by giving the memory limit
                    // some additional buffer.
                    long memoryThresholdForFallbackBytes = memoryLimitBytes * 8 / 10;
                    if (memoryUsageBytes >= memoryThresholdForFallbackBytes)
                    {
                        return 0;
                    }

                    // Determine how many threads can be added without exceeding the memory threshold
                    long achievableNumThreadsGoal =
                        counts.NumExistingThreads +
                        (memoryThresholdForFallbackBytes - memoryUsageBytes) /
                            WorkerThread.EstimatedAdditionalStackUsagePerThreadBytes;
                    newNumThreadsGoal = (short)Math.Min(newNumThreadsGoal, achievableNumThreadsGoal);
                    if (newNumThreadsGoal <= numThreadsGoal)
                    {
                        return 0;
                    }
                } while (false);

                _numThreadsAddedDueToBlocking += (short)(newNumThreadsGoal - numThreadsGoal);
                counts = _separated.counts.InterlockedSetNumThreadsGoal(newNumThreadsGoal);
                // HillClimbing.ThreadPoolHillClimber.ForceChange(
                //     newNumThreadsGoal,
                //     HillClimbing.StateOrTransition.CooperativeBlocking);
                if (counts.NumProcessingWork >= numThreadsGoal && _separated.numRequestedWorkers > 0)
                {
                    addWorker = true;
                }

                numThreadsGoal = newNumThreadsGoal;
                if (numThreadsGoal >= targetThreadsGoal)
                {
                    return 0;
                }
            } while (false);

            // Calculate how much delay to induce before another thread is created. These operations don't overflow because of
            // limits on max thread count and max delays.
            _pendingBlockingAdjustment = PendingBlockingAdjustment.WithDelayIfNecessary;
            int delayStepCount = 1 + (numThreadsGoal - configuredMaxThreadsWithoutDelay) / BlockingConfig.ThreadsPerDelayStep;
            return Math.Min((uint)delayStepCount * BlockingConfig.DelayStepMs, BlockingConfig.MaxDelayMs);
        }

        private enum PendingBlockingAdjustment : byte
        {
            None,
            Immediately,
            WithDelayIfNecessary
        }

        private static class BlockingConfig
        {
            public static readonly short ThreadsToAddWithoutDelay;
            public static readonly short ThreadsPerDelayStep;
            public static readonly uint DelayStepMs;
            public static readonly uint MaxDelayMs;

#pragma warning disable CA1810 // remove the explicit static constructor
            static BlockingConfig()
            {
                // Summary description of how blocking compensation works and how the config settings below are used:
                // - After the thread count based on MinThreads is reached, up to ThreadsToAddWithoutDelay additional threads
                //   may be created without a delay
                // - After that, before each additional thread is created, a delay is induced, starting with DelayStepMs
                // - For every ThreadsPerDelayStep threads that are added with a delay, an additional DelayStepMs is added to
                //   the delay
                // - The delay may not exceed MaxDelayMs
                // - Delays are only induced before creating threads. If threads are already available, they would be released
                //   without delay to compensate for cooperative blocking.
                // - Physical memory usage and limits are also used and beyond a threshold, the system switches to fallback mode
                //   where threads would be created if starvation is detected, typically with higher delays

                // After the thread count based on MinThreads is reached, this value (after it is multiplied by the processor
                // count) specifies how many additional threads may be created without a delay
                int blocking_threadsToAddWithoutDelay_procCountFactor = 1;

                // After the thread count based on ThreadsToAddWithoutDelay is reached, this value (after it is multiplied by
                // the processor count) specifies after how many threads an additional DelayStepMs would be added to the delay
                // before each new thread is created
                int blocking_threadsPerDelayStep_procCountFactor = 1;

                // After the thread count based on ThreadsToAddWithoutDelay is reached, this value specifies how much additional
                // delay to add per ThreadsPerDelayStep threads, which would be applied before each new thread is created
                DelayStepMs = 25;

                // After the thread count based on ThreadsToAddWithoutDelay is reached, this value specifies the max delay to
                // use before each new thread is created
                MaxDelayMs = 250;

                int processorCount = Environment.ProcessorCount;
                ThreadsToAddWithoutDelay = (short)(processorCount * blocking_threadsToAddWithoutDelay_procCountFactor);
                if (ThreadsToAddWithoutDelay > MaxPossibleThreadCount ||
                    ThreadsToAddWithoutDelay / processorCount != blocking_threadsToAddWithoutDelay_procCountFactor)
                {
                    ThreadsToAddWithoutDelay = MaxPossibleThreadCount;
                }

                blocking_threadsPerDelayStep_procCountFactor = Math.Max(1, blocking_threadsPerDelayStep_procCountFactor);
                short maxThreadsPerDelayStep = (short)(MaxPossibleThreadCount - ThreadsToAddWithoutDelay);
                ThreadsPerDelayStep =
                    (short)(processorCount * blocking_threadsPerDelayStep_procCountFactor);
                if (ThreadsPerDelayStep > maxThreadsPerDelayStep ||
                    ThreadsPerDelayStep / processorCount != blocking_threadsPerDelayStep_procCountFactor)
                {
                    ThreadsPerDelayStep = maxThreadsPerDelayStep;
                }

                MaxDelayMs = Math.Max(1, Math.Min(MaxDelayMs, GateThread.GateActivitiesPeriodMs));
                DelayStepMs = Math.Max(1, Math.Min(DelayStepMs, MaxDelayMs));
            }
#pragma warning restore CA1810
        }
    }
}

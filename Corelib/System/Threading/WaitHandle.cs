// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Threading
{
    public abstract class WaitHandle : IDisposable
    {
        internal const int MaxWaitHandles = 64;

        protected static readonly IntPtr InvalidHandle = new IntPtr(-1);

        // IMPORTANT:
        // - Do not add or rearrange fields as the EE depends on this layout.

        internal WaitSubsystem.WaitableObject? _waitHandle;

        [ThreadStatic]
        private static WaitSubsystem.WaitableObject?[]? t_safeWaitHandlesForRent;

        // The wait result values below match Win32 wait result codes (WAIT_OBJECT_0,
        // WAIT_ABANDONED, WAIT_TIMEOUT).

        // Successful wait on first object. When waiting for multiple objects the
        // return value is (WaitSuccess + waitIndex).
        internal const int WaitSuccess = 0;

        // The specified object is a mutex object that was not released by the
        // thread that owned the mutex object before the owning thread terminated.
        // When waiting for multiple objects the return value is (WaitAbandoned +
        // waitIndex).
        internal const int WaitAbandoned = 0x80;

        public const int WaitTimeout = 0x102;

        protected WaitHandle()
        {
        }

        internal static int ToTimeoutMilliseconds(TimeSpan timeout)
        {
            long timeoutMilliseconds = (long)timeout.TotalMilliseconds;
            if (timeoutMilliseconds < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), ArgumentOutOfRangeException.NeedNonNegOrNegative1);
            }
            if (timeoutMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), ArgumentOutOfRangeException.LessEqualToIntegerMaxVal);
            }
            return (int)timeoutMilliseconds;
        }

        public virtual void Close() => Dispose();

        protected virtual void Dispose(bool explicitDisposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual bool WaitOne(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), ArgumentOutOfRangeException.NeedNonNegOrNegative1);
            }

            return WaitOneNoCheck(millisecondsTimeout);
        }

        private bool WaitOneNoCheck(int millisecondsTimeout)
        {
            Debug.Assert(millisecondsTimeout >= -1);

            // The field value is modifiable via the public <see cref="WaitHandle.SafeWaitHandle"/> property, save it locally
            // to ensure that one instance is used in all places in this method
            WaitSubsystem.WaitableObject waitHandle = _waitHandle ?? throw new ObjectDisposedException(null, ObjectDisposedException.Generic);

            int waitResult;
            
            waitResult = WaitOneCore(waitHandle, millisecondsTimeout);

            if (waitResult == WaitAbandoned)
            {
                throw new AbandonedMutexException();
            }

            return waitResult != WaitTimeout;
        }

        // Returns an array for storing SafeWaitHandles in WaitMultiple calls. The array
        // is reused for subsequent calls to reduce GC pressure.
        private static WaitSubsystem.WaitableObject?[] RentSafeWaitHandleArray(int capacity)
        {
            WaitSubsystem.WaitableObject?[]? safeWaitHandles = t_safeWaitHandlesForRent;

            t_safeWaitHandlesForRent = null;

            // t_safeWaitHandlesForRent can be null when it was not initialized yet or
            // if a re-entrant wait is performed and the array is already rented. In
            // that case we just allocate a new one and reuse it as necessary.
            int currentLength = (safeWaitHandles != null) ? safeWaitHandles.Length : 0;
            if (currentLength < capacity)
            {
                safeWaitHandles = new WaitSubsystem.WaitableObject[Math.Max(capacity,
                    Math.Min(MaxWaitHandles, 2 * currentLength))];
            }

            return safeWaitHandles!;
        }

        private static void ReturnSafeWaitHandleArray(WaitSubsystem.WaitableObject?[]? safeWaitHandles)
            => t_safeWaitHandlesForRent = safeWaitHandles;

        /// <summary>
        /// Obtains all of the corresponding safe wait handles and adds a ref to each. Since the <see cref="SafeWaitHandle"/>
        /// property is publically modifiable, this makes sure that we add and release refs one the same set of safe wait
        /// handles to keep them alive during a multi-wait operation.
        /// </summary>
        private static void ObtainSafeWaitHandles(
            ReadOnlySpan<WaitHandle> waitHandles,
            Span<WaitSubsystem.WaitableObject?> safeWaitHandles)
        {
            Debug.Assert(waitHandles.Length > 0);
            Debug.Assert(waitHandles.Length <= MaxWaitHandles);

            bool lastSuccess = true;
            WaitSubsystem.WaitableObject? lastSafeWaitHandle = null;
            try
            {
                for (int i = 0; i < waitHandles.Length; ++i)
                {
                    WaitHandle waitHandle = waitHandles[i];
                    if (waitHandle == null)
                    {
                        throw new ArgumentNullException("waitHandles[" + i + ']', "At least one element in the specified array was null.");
                    }

                    WaitSubsystem.WaitableObject safeWaitHandle = waitHandle._waitHandle ??
                        // Throw ObjectDisposedException for backward compatibility even though it is not representative of the issue
                        throw new ObjectDisposedException(null, ObjectDisposedException.Generic);

                    lastSafeWaitHandle = safeWaitHandle;
                    lastSuccess = false;
                    safeWaitHandles[i] = safeWaitHandle;
                }
            }
            catch
            {
                for (int i = 0; i < waitHandles.Length; ++i)
                {
                    WaitSubsystem.WaitableObject? safeWaitHandle = safeWaitHandles[i];
                    if (safeWaitHandle == null)
                    {
                        break;
                    }
                    safeWaitHandles[i] = null;
                    if (safeWaitHandle == lastSafeWaitHandle)
                    {
                        lastSafeWaitHandle = null;
                        lastSuccess = true;
                    }
                }

                if (!lastSuccess)
                {
                    Debug.Assert(lastSafeWaitHandle != null);
                }

                throw;
            }
        }

        private static int WaitMultiple(WaitHandle[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            if (waitHandles == null)
            {
                throw new ArgumentNullException(nameof(waitHandles), "The waitHandles parameter cannot be null.");
            }

            return WaitMultiple(new ReadOnlySpan<WaitHandle>(waitHandles), waitAll, millisecondsTimeout);
        }

        private static int WaitMultiple(ReadOnlySpan<WaitHandle> waitHandles, bool waitAll, int millisecondsTimeout)
        {
            if (waitHandles.Length == 0)
            {
                throw new ArgumentException("Waithandle array may not be empty.", nameof(waitHandles));
            }
            if (waitHandles.Length > MaxWaitHandles)
            {
                throw new NotSupportedException("The number of WaitHandles must be less than or equal to 64.");
            }
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), ArgumentOutOfRangeException.NeedNonNegOrNegative1);
            }

            WaitSubsystem.WaitableObject?[]? safeWaitHandles = RentSafeWaitHandleArray(waitHandles.Length);

            try
            {
                int waitResult;

                ObtainSafeWaitHandles(waitHandles, safeWaitHandles);
                waitResult = WaitMultipleIgnoringSyncContext(safeWaitHandles, waitAll, millisecondsTimeout);

                if (waitResult >= WaitAbandoned && waitResult < WaitAbandoned + waitHandles.Length)
                {
                    if (waitAll)
                    {
                        // In the case of WaitAll the OS will only provide the information that mutex was abandoned.
                        // It won't tell us which one.  So we can't set the Index or provide access to the Mutex
                        throw new AbandonedMutexException();
                    }

                    waitResult -= WaitAbandoned;
                    throw new AbandonedMutexException(waitResult, waitHandles[waitResult]);
                }

                return waitResult;
            }
            finally
            {
                for (int i = 0; i < waitHandles.Length; ++i)
                {
                    if (safeWaitHandles[i] is WaitSubsystem.WaitableObject swh)
                    {
                        safeWaitHandles[i] = null;
                    }
                }

                ReturnSafeWaitHandleArray(safeWaitHandles);
            }
        }

        private static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, int millisecondsTimeout)
        {
            if (toSignal == null)
            {
                throw new ArgumentNullException(nameof(toSignal));
            }
            if (toWaitOn == null)
            {
                throw new ArgumentNullException(nameof(toWaitOn));
            }
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), ArgumentOutOfRangeException.NeedNonNegOrNegative1);
            }

            // The field value is modifiable via the public <see cref="WaitHandle.SafeWaitHandle"/> property, save it locally
            // to ensure that one instance is used in all places in this method
            WaitSubsystem.WaitableObject? safeWaitHandleToSignal = toSignal._waitHandle;
            WaitSubsystem.WaitableObject? safeWaitHandleToWaitOn = toWaitOn._waitHandle;
            if (safeWaitHandleToSignal == null || safeWaitHandleToWaitOn == null)
            {
                // Throw ObjectDisposedException for backward compatibility even though it is not be representative of the issue
                throw new ObjectDisposedException(null, ObjectDisposedException.Generic);
            }

            int ret = SignalAndWaitCore(
                safeWaitHandleToSignal,
                safeWaitHandleToWaitOn,
                millisecondsTimeout);

            if (ret == WaitAbandoned)
            {
                throw new AbandonedMutexException();
            }

            return ret != WaitTimeout;
        }

        internal static void ThrowInvalidHandleException()
        {
            var ex = new InvalidOperationException("The handle is invalid.");
            throw ex;
        }

        public virtual bool WaitOne(TimeSpan timeout) => WaitOneNoCheck(ToTimeoutMilliseconds(timeout));
        public virtual bool WaitOne() => WaitOneNoCheck(-1);
        public virtual bool WaitOne(int millisecondsTimeout, bool exitContext) => WaitOne(millisecondsTimeout);
        public virtual bool WaitOne(TimeSpan timeout, bool exitContext) => WaitOneNoCheck(ToTimeoutMilliseconds(timeout));

        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout) =>
            WaitMultiple(waitHandles, true, millisecondsTimeout) != WaitTimeout;
        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout) =>
            WaitMultiple(waitHandles, true, ToTimeoutMilliseconds(timeout)) != WaitTimeout;
        public static bool WaitAll(WaitHandle[] waitHandles) =>
            WaitMultiple(waitHandles, true, -1) != WaitTimeout;
        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) =>
            WaitMultiple(waitHandles, true, millisecondsTimeout) != WaitTimeout;
        public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext) =>
            WaitMultiple(waitHandles, true, ToTimeoutMilliseconds(timeout)) != WaitTimeout;

        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout) =>
            WaitMultiple(waitHandles, false, millisecondsTimeout);
        internal static int WaitAny(ReadOnlySpan<WaitHandle> waitHandles, int millisecondsTimeout) =>
            WaitMultiple(waitHandles, false, millisecondsTimeout);
        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout) =>
            WaitMultiple(waitHandles, false, ToTimeoutMilliseconds(timeout));
        public static int WaitAny(WaitHandle[] waitHandles) =>
            WaitMultiple(waitHandles, false, -1);
        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) =>
            WaitMultiple(waitHandles, false, millisecondsTimeout);
        public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext) =>
            WaitMultiple(waitHandles, false, ToTimeoutMilliseconds(timeout));

        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn) =>
            SignalAndWait(toSignal, toWaitOn, -1);
        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, TimeSpan timeout, bool exitContext) =>
            SignalAndWait(toSignal, toWaitOn, ToTimeoutMilliseconds(timeout));
        public static bool SignalAndWait(WaitHandle toSignal, WaitHandle toWaitOn, int millisecondsTimeout, bool exitContext) =>
            SignalAndWait(toSignal, toWaitOn, millisecondsTimeout);
        
        
        
        private static int WaitOneCore(WaitSubsystem.WaitableObject handle, int millisecondsTimeout) =>
            WaitSubsystem.Wait(handle, millisecondsTimeout, true);

        internal static int WaitMultipleIgnoringSyncContext(Span<WaitSubsystem.WaitableObject> handles, bool waitAll, int millisecondsTimeout) =>
            WaitSubsystem.Wait(handles, waitAll, millisecondsTimeout);

        private static int SignalAndWaitCore(WaitSubsystem.WaitableObject handleToSignal, WaitSubsystem.WaitableObject handleToWaitOn, int millisecondsTimeout) =>
            WaitSubsystem.SignalAndWait(handleToSignal, handleToWaitOn, millisecondsTimeout);

    }
}

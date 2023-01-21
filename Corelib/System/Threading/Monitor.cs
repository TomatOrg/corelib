// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace System.Threading
{
    public static class Monitor
    {
        public static bool TryEnter(object obj, TimeSpan timeout)
            => TryEnter(obj, WaitHandle.ToTimeoutMilliseconds(timeout));

        public static void TryEnter(object obj, TimeSpan timeout, ref bool lockTaken)
            => TryEnter(obj, WaitHandle.ToTimeoutMilliseconds(timeout), ref lockTaken);

        public static bool Wait(object obj, TimeSpan timeout) => Wait(obj, WaitHandle.ToTimeoutMilliseconds(timeout));

        public static bool Wait(object obj) => Wait(obj, Timeout.Infinite);

        /// <summary>
        /// Obtain the monitor mutex of obj. Will block if another thread holds the mutex
        /// Will not block if the current thread holds the mutex,
        /// however the caller must ensure that the same number of Exit
        /// calls are made as there were Enter calls.
        /// </summary>
        [MethodImpl(MethodCodeType = MethodCodeType.Native)]
        public static extern void Enter(object obj);
        
        // Use a ref bool instead of out to ensure that unverifiable code must
        // initialize this value to something.  If we used out, the value
        // could be uninitialized if we threw an exception in our prolog.
        // The JIT should inline this method to allow check of lockTaken argument to be optimized out
        // in the typical case. Note that the method has to be transparent for inlining to be allowed by the VM.
        public static void Enter(object obj, ref bool lockTaken)
        {
            if (lockTaken)
                ThrowLockTakenException();

            ReliableEnter(obj, ref lockTaken);
            Debug.Assert(lockTaken);
        }

        [DoesNotReturn]
        private static void ThrowLockTakenException()
        {
            throw new ArgumentException("Argument must be initialized to false", "lockTaken");
        }

        [MethodImpl(MethodCodeType = MethodCodeType.Native)]
        private static extern void ReliableEnter(object obj, ref bool lockTaken);


        /// <summary>
        /// Release the monitor mutex. If one or more threads are waiting to acquire the
        /// mutex, and the current thread has executed as many Exits as
        /// Enters, one of the threads will be unblocked and allowed to proceed.
        ///
        /// Exceptions: ArgumentNullException if object is null.
        ///             SynchronizationLockException if the current thread does not
        ///             own the mutex.
        /// </summary>
        [MethodImpl(MethodCodeType = MethodCodeType.Native)]
        public static extern void Exit(object obj);

        /// <summary>
        /// Similar to Enter, but will never block. That is, if the current thread can
        /// acquire the monitor mutex without blocking, it will do so and TRUE will
        /// be returned. Otherwise FALSE will be returned.
        ///
        /// Exceptions: ArgumentNullException if object is null.
        /// </summary>
        public static bool TryEnter(object obj)
        {
            bool lockTaken = false;
            TryEnter(obj, 0, ref lockTaken);
            return lockTaken;
        }

        // The JIT should inline this method to allow check of lockTaken argument to be optimized out
        // in the typical case. Note that the method has to be transparent for inlining to be allowed by the VM.
        public static void TryEnter(object obj, ref bool lockTaken)
        {
            if (lockTaken)
                ThrowLockTakenException();

            ReliableEnterTimeout(obj, 0, ref lockTaken);
        }

        /// <summary>
        /// Version of TryEnter that will block, but only up to a timeout period
        /// expressed in milliseconds. If timeout == Timeout.Infinite the method
        /// becomes equivalent to Enter.
        ///
        /// Exceptions: ArgumentNullException if object is null.
        ///             ArgumentException if timeout < -1 (Timeout.Infinite).
        /// </summary>
        // The JIT should inline this method to allow check of lockTaken argument to be optimized out
        // in the typical case. Note that the method has to be transparent for inlining to be allowed by the VM.
        public static bool TryEnter(object obj, int millisecondsTimeout)
        {
            bool lockTaken = false;
            TryEnter(obj, millisecondsTimeout, ref lockTaken);
            return lockTaken;
        }

        // The JIT should inline this method to allow check of lockTaken argument to be optimized out
        // in the typical case. Note that the method has to be transparent for inlining to be allowed by the VM.
        public static void TryEnter(object obj, int millisecondsTimeout, ref bool lockTaken)
        {
            if (lockTaken)
                ThrowLockTakenException();

            ReliableEnterTimeout(obj, millisecondsTimeout, ref lockTaken);
        }

        [MethodImpl(MethodCodeType = MethodCodeType.Native)]
        private static extern void ReliableEnterTimeout(object obj, int timeout, ref bool lockTaken);

        public static bool IsEntered(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return IsEnteredNative(obj);
        }

        [MethodImpl(MethodCodeType = MethodCodeType.Native)]
        private static extern bool IsEnteredNative(object obj);
        
        public static bool Wait(object obj, int millisecondsTimeout)
        {
            if (obj == null)
                throw (new ArgumentNullException(nameof(obj)));
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout),
                    ArgumentOutOfRangeException.NeedNonNegOrNegative1);

            if (!IsEnteredNative(obj))
            {
                throw new SynchronizationLockException();
            }

            return obj._condition.Wait(ref obj._mutex, millisecondsTimeout);
        }

        public static void Pulse(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            
            if (!IsEnteredNative(obj))
            {
                throw new SynchronizationLockException();
            }
            
            obj._condition.NotifyOne();
        }

        public static void PulseAll(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (!IsEnteredNative(obj))
            {
                throw new SynchronizationLockException();
            }
            
            obj._condition.NotifyAll();
        }

        /// <summary>
        /// Gets the number of times there was contention upon trying to take a <see cref="Monitor"/>'s mutex so far.
        /// </summary>
        public static long LockContentionCount => GetLockContentionCount();

        private static extern long GetLockContentionCount();
    }
}
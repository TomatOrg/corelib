// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;

namespace System.Threading
{
    public sealed partial class Semaphore : WaitHandle
    {
        // creates a nameless semaphore object
        // Win32 only takes maximum count of int.MaxValue
        public Semaphore(int initialCount, int maximumCount) : this(initialCount, maximumCount, null) { }

        public Semaphore(int initialCount, int maximumCount, string? name) :
            this(initialCount, maximumCount, name, out _)
        {
        }

        public Semaphore(int initialCount, int maximumCount, string? name, out bool createdNew)
        {
            if (initialCount < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCount), ArgumentOutOfRangeException.NeedNonNegNum);

            if (maximumCount < 1)
                throw new ArgumentOutOfRangeException(nameof(maximumCount), ArgumentOutOfRangeException.NeedPosNum);

            if (initialCount > maximumCount)
                throw new ArgumentException("The initial count for the semaphore must be greater than or equal to zero and less than the maximum count.");

            CreateSemaphoreCore(initialCount, maximumCount, name, out createdNew);
        }

        public static Semaphore OpenExisting(string name)
        {
            switch (OpenExistingWorker(name, out Semaphore? result))
            {
                case OpenExistingResult.NameNotFound:
                    throw new WaitHandleCannotBeOpenedException();
                case OpenExistingResult.NameInvalid:
                    throw new WaitHandleCannotBeOpenedException(
                        $"A WaitHandle with system-wide name '{name}' cannot be created. A WaitHandle of a different type might have the same name.");

                default:
                    Debug.Assert(result != null, "result should be non-null on success");
                    return result;
            }
        }

        public int Release() => ReleaseCore(1);

        // increase the count on a semaphore, returns previous count
        public int Release(int releaseCount)
        {
            if (releaseCount < 1)
                throw new ArgumentOutOfRangeException(nameof(releaseCount), ArgumentOutOfRangeException.NeedNonNegNum);

            return ReleaseCore(releaseCount);
        }
        
        private void CreateSemaphoreCore(int initialCount, int maximumCount, string? name, out bool createdNew)
        {
            if (name != null)
            {
                throw new PlatformNotSupportedException(PlatformNotSupportedException.NamedSynchronizationPrimitives);
            }

            _waitHandle = WaitSubsystem.NewSemaphore(initialCount, maximumCount);
            createdNew = true;
        }

        private static OpenExistingResult OpenExistingWorker(string name, out Semaphore? result)
        {
            throw new PlatformNotSupportedException(PlatformNotSupportedException.NamedSynchronizationPrimitives);
        }

        private int ReleaseCore(int releaseCount)
        {
            return WaitSubsystem.ReleaseSemaphore(_waitHandle, releaseCount);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Threading
{
    /// <summary>
    /// Synchronization primitive that can also be used for interprocess synchronization
    /// </summary>
    public sealed partial class Mutex : WaitHandle
    {
        public Mutex(bool initiallyOwned, string? name, out bool createdNew)
        {
            CreateMutexCore(initiallyOwned, name, out createdNew);
        }

        public Mutex(bool initiallyOwned, string? name)
        {
            CreateMutexCore(initiallyOwned, name, out _);
        }

        public Mutex(bool initiallyOwned)
        {
            CreateMutexCore(initiallyOwned, null, out _);
        }

        public Mutex()
        {
            CreateMutexCore(false, null, out _);
        }

        private Mutex(WaitSubsystem.WaitableObject handle)
        {
            _waitHandle = handle;
        }

        public static Mutex OpenExisting(string name)
        {
            switch (OpenExistingWorker(name, out Mutex? result))
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

        public static bool TryOpenExisting(string name, [NotNullWhen(true)] out Mutex? result) =>
            OpenExistingWorker(name, out result!) == OpenExistingResult.Success;
        
        private void CreateMutexCore(bool initiallyOwned, string? name, out bool createdNew)
        {
            if (name != null)
            {
                WaitSubsystem.WaitableObject? safeWaitHandle = WaitSubsystem.CreateNamedMutex(initiallyOwned, name, out createdNew);
                if (safeWaitHandle == null)
                {
                    throw new WaitHandleCannotBeOpenedException(
                        $"A WaitHandle with system-wide name '{name}' cannot be created. A WaitHandle of a different type might have the same name.");
                }
                _waitHandle = safeWaitHandle;
                return;
            }

            _waitHandle = WaitSubsystem.NewMutex(initiallyOwned);
            createdNew = true;
        }

        private static OpenExistingResult OpenExistingWorker(string name, out Mutex? result)
        {
            OpenExistingResult status = WaitSubsystem.OpenNamedMutex(name, out WaitSubsystem.WaitableObject? safeWaitHandle);
            result = status == OpenExistingResult.Success ? new Mutex(safeWaitHandle!) : null;
            return status;
        }

        public void ReleaseMutex()
        {
            // The field value is modifiable via the public <see cref="WaitHandle.SafeWaitHandle"/> property, save it locally
            // to ensure that one instance is used in all places in this method
            WaitSubsystem.ReleaseMutex(_waitHandle);
        }
    }
}

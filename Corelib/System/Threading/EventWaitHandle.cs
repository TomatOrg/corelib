// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Versioning;

namespace System.Threading
{
    public partial class EventWaitHandle : WaitHandle
    {
        public EventWaitHandle(bool initialState, EventResetMode mode) :
            this(initialState, mode, null, out _)
        {
        }

        public EventWaitHandle(bool initialState, EventResetMode mode, string? name) :
            this(initialState, mode, name, out _)
        {
        }

        public EventWaitHandle(bool initialState, EventResetMode mode, string? name, out bool createdNew)
        {
            if (mode != EventResetMode.AutoReset && mode != EventResetMode.ManualReset)
                throw new ArgumentException(ArgumentException.InvalidFlag, nameof(mode));

            CreateEventCore(initialState, mode, name, out createdNew);
        }

        private void CreateEventCore(bool initialState, EventResetMode mode, string? name, out bool createdNew)
        {
            if (name != null)
                throw new PlatformNotSupportedException(PlatformNotSupportedException.NamedSynchronizationPrimitives);

            _waitHandle = WaitSubsystem.NewEvent(initialState, mode);
            createdNew = true;
        }

        private static OpenExistingResult OpenExistingWorker(string name, out EventWaitHandle? result)
        {
            throw new PlatformNotSupportedException(PlatformNotSupportedException.NamedSynchronizationPrimitives);
        }

        public bool Reset()
        {
            WaitSubsystem.ResetEvent(_waitHandle);
            return true;
        }

        public bool Set()
        {
            WaitSubsystem.SetEvent(_waitHandle);
            return true;
        }

        internal static bool Set(WaitSubsystem.WaitableObject waitHandle)
        {
            WaitSubsystem.SetEvent(waitHandle);
            return true;
        }

    }
}

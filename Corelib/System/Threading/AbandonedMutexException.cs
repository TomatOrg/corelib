// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.Threading
{
    public class AbandonedMutexException : SystemException
    {
        private int _mutexIndex = -1;
        private Mutex? _mutex;

        public AbandonedMutexException()
            : base("The wait completed due to an abandoned mutex.")
        {
        }

        public AbandonedMutexException(string? message)
            : base(message)
        {
        }

        public AbandonedMutexException(string? message, Exception? inner)
            : base(message, inner)
        {
        }

        public AbandonedMutexException(int location, WaitHandle? handle)
            : base("The wait completed due to an abandoned mutex.")
        {
            SetupException(location, handle);
        }

        public AbandonedMutexException(string? message, int location, WaitHandle? handle)
            : base(message)
        {
            SetupException(location, handle);
        }

        public AbandonedMutexException(string? message, Exception? inner, int location, WaitHandle? handle)
            : base(message, inner)
        {
            SetupException(location, handle);
        }

        private void SetupException(int location, WaitHandle? handle)
        {
            _mutexIndex = location;
            _mutex = handle as Mutex;
        }

        public Mutex? Mutex => _mutex;
        public int MutexIndex => _mutexIndex;
    }
}

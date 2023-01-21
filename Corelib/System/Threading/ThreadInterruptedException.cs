// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.Threading
{
    /// <summary>
    /// An exception class to indicate that the thread was interrupted from a waiting state.
    /// </summary>
    public class ThreadInterruptedException : SystemException
    {
        public ThreadInterruptedException() : base("Thread was interrupted from a waiting state.")
        {
        }

        public ThreadInterruptedException(string? message)
            : base(message)
        {
        }

        public ThreadInterruptedException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

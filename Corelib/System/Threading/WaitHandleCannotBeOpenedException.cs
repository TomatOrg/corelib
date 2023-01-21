// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.Threading
{
    public class WaitHandleCannotBeOpenedException : ApplicationException
    {
        public WaitHandleCannotBeOpenedException() : base("No handle of the given name exists.")
        {
        }

        public WaitHandleCannotBeOpenedException(string? message) : base(message)
        {
        }

        public WaitHandleCannotBeOpenedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}

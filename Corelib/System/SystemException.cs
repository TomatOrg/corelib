// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System
{
    public class SystemException : Exception
    {
        public SystemException()
            : base("System error.")
        {
        }

        public SystemException(string? message)
            : base(message)
        {
        }

        public SystemException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

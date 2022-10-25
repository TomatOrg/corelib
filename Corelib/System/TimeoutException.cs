// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*=============================================================================
**
**
**
** Purpose: Exception class for Timeout
**
**
=============================================================================*/

using System.Runtime.Serialization;

namespace System
{
    public class TimeoutException : SystemException
    {
        public TimeoutException()
            : base("The operation has timed out.")
        {
        }

        public TimeoutException(string? message)
            : base(message)
        {
        }

        public TimeoutException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*=============================================================================
**
**
**
** Purpose: Exception class for duplicate objects in WaitAll/WaitAny.
**
**
=============================================================================*/

using System.Runtime.Serialization;

namespace System
{
    // The DuplicateWaitObjectException is thrown when an object
    // appears more than once in the list of objects to WaitAll or WaitAny.
    public class DuplicateWaitObjectException : ArgumentException
    {
        // Creates a new DuplicateWaitObjectException with its message
        // string set to a default message.
        public DuplicateWaitObjectException()
            : base("Duplicate objects in argument.")
        {
        }

        public DuplicateWaitObjectException(string? parameterName)
            : base("Duplicate objects in argument.", parameterName)
        {
        }

        public DuplicateWaitObjectException(string? parameterName, string? message)
            : base(message, parameterName)
        {
        }

        public DuplicateWaitObjectException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

    }
}

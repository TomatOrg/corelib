// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*============================================================
**
**
**
** Purpose: An exception for OS 'access denied' types of
**          errors, including IO and limited security types
**          of errors.
**
**
===========================================================*/

using System.Runtime.Serialization;

namespace System
{
    // The UnauthorizedAccessException is thrown when access errors
    // occur from IO or other OS methods.
    public class UnauthorizedAccessException : SystemException
    {
        public UnauthorizedAccessException()
            : base("Attempted to perform an unauthorized operation.")
        {
        }

        public UnauthorizedAccessException(string? message)
            : base(message)
        {
        }

        public UnauthorizedAccessException(string? message, Exception? inner)
            : base(message, inner)
        {
        }
    }
}

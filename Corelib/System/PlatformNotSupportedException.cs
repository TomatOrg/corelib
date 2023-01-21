// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*=============================================================================
**
**
**
** Purpose: To handle features that don't run on particular platforms
**
**
=============================================================================*/

using System.Runtime.Serialization;

namespace System
{
    public class PlatformNotSupportedException : NotSupportedException
    {

        internal const string NamedSynchronizationPrimitives =
            "The named version of this synchronization primitive is not supported on this platform.";
        
        public PlatformNotSupportedException()
            : base("Operation is not supported on this platform.")
        {
        }

        public PlatformNotSupportedException(string? message)
            : base(message)
        {
        }

        public PlatformNotSupportedException(string? message, Exception? inner)
            : base(message, inner)
        {
        }

    }
}

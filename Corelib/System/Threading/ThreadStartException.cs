// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.Threading
{
    public sealed class ThreadStartException : SystemException
    {
        internal ThreadStartException()
            : base("Thread failed to start.")
        {
        }

        internal ThreadStartException(Exception reason)
            : base("Thread failed to start.", reason)
        {
        }
    }
}

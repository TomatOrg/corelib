// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Threading
{
    internal sealed partial class PortableThreadPool
    {
        private struct CpuUtilizationReader
        {
            public int CurrentUtilization => 0;
        }
    }
}

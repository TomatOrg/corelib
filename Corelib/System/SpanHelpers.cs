// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace System
{
    internal static partial class SpanHelpers
    {
        internal static void ClearWithoutReferences(ref byte b, nuint byteLength)
        {
            if (byteLength == 0)
                return;
            
            Buffer._ZeroMemory(ref b, byteLength);
        }
    }
}

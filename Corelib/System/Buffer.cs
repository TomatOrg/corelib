using System.Runtime.CompilerServices;

namespace System;

internal static class Buffer
{

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    internal static extern void Memmove(ref byte destination, ref byte source, nuint elementCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Memmove<T>(ref T destination, ref T source, nuint elementCount)
        where T : unmanaged
    {
        Memmove(
            ref Unsafe.As<T, byte>(ref destination),
            ref Unsafe.As<T, byte>(ref source),
            elementCount * (nuint)Unsafe.SizeOf<T>());
    }
    
    internal static unsafe void ZeroMemory(byte* dest, nuint len)
    {
        SpanHelpers.ClearWithoutReferences(ref *dest, len);
    }

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    internal static extern void _ZeroMemory(ref byte b, nuint byteLength);

}
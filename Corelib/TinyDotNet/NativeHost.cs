using System;
using System.Runtime.CompilerServices;

namespace TinyDotNet;

/// <summary>
/// A collections of function that are required to be provided by
/// the native host and not by TinyDotNet itself
/// </summary>
internal static class NativeHost
{
    
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern bool WaitableSend(ulong waitable, bool block);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern int WaitableWait(ulong waitable, bool block);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern void WaitableClose(ulong waitable);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern void WaitableOpen(ulong waitable);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern ulong WaitableSelect(ref Span<ulong> waitables, int sendCount, int waitCount, bool block);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern ulong CreateWaitable(int count);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern ulong WaitableAfter(long timeoutMicro);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern ulong PutWaitable(ulong waitable);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal static extern void ReleaseWaitable(ulong waitable);

}
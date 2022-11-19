using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TinyDotNet.Sync;

[StructLayout(LayoutKind.Sequential)]
internal struct Mutex
{

    private int _state;
    private Semaphore _semaphore;

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    internal extern void Lock();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    internal extern void Unlock();

}
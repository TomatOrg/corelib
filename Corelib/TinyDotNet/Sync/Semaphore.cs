using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TinyDotNet.Sync;

[StructLayout(LayoutKind.Sequential)]
internal struct Semaphore
{

    private uint _value;
    private Spinlock _lock;
    private unsafe void* _waiters;
    private uint _nwait;

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    public extern void Acquire(bool lifo);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    public extern void Release(bool handoff);

}
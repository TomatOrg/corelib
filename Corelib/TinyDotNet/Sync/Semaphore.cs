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
    internal extern bool Acquire(bool lifo, long timeout);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    internal extern void Release(bool handoff);

}
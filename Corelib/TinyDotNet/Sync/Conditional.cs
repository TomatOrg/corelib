using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TinyDotNet.Sync;

[StructLayout(LayoutKind.Sequential)]
internal struct Conditional
{

    private NotifyList _notify;

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    public extern void Wait(ref Mutex mutex);
    
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    public extern void Signal();
    
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    public extern void Broadcast();

}
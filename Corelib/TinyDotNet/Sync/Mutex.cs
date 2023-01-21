using System.Runtime.CompilerServices;

namespace TinyDotNet.Sync;

internal struct Mutex
{

    private byte _byte;

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    internal extern void Lock();
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    internal extern void Unlock();

}
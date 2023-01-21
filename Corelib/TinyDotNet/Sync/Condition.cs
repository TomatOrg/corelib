using System.Runtime.CompilerServices;

namespace TinyDotNet.Sync;

internal struct Condition
{

    private byte _byte;

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    internal extern bool Wait(ref Mutex mutex, long timeoutMilliseconds);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    internal extern bool NotifyOne();

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    internal extern void NotifyAll();

}
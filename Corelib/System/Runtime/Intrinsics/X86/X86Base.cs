using System.Runtime.CompilerServices;

namespace System.Runtime.Intrinsics.X86;

public static class X86Base
{

    public static bool IsSupported => true;
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern void Pause();

}
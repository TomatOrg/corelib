using System.Reflection;
using System.Runtime.InteropServices;

namespace TinyDotNet.Reflection;

[StructLayout(LayoutKind.Sequential)]
public class MethodSpec
{

    private int _method;
    private byte[] _instantiation;

}
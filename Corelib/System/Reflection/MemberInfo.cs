using System.Runtime.InteropServices;

namespace System.Reflection;

[StructLayout(LayoutKind.Sequential)]
public abstract class MemberInfo
{

    internal Type _declaringType;
    internal Module _module;
    internal string _name;

    public Type DeclaringType => _declaringType;
    public Module Module => _module;
    public string Name => _name;

    internal MemberInfo()
    {
    }

}
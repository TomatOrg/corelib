using System.IO;
using System.Runtime.InteropServices;

namespace System.Reflection;

[StructLayout(LayoutKind.Sequential)]
public class ManifestResourceInfo
{

    private string _filename;
    private Assembly _referencedAssembly;
    private ResourceLocation _resourceLocation;
    private byte[] _data;

    public string FileName => _filename;
    public Assembly ReferencedAssembly => _referencedAssembly;
    public ResourceLocation ResourceLocation => _resourceLocation;

    internal Stream AsStream()
    {
        return new MemoryStream(_data, 0, _data.Length, writable: false, publiclyVisible: false);
    }
    
}
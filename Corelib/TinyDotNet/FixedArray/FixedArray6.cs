using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TinyDotNet;

[StructLayout(LayoutKind.Sequential)]
public struct FixedArray6<T> where T : unmanaged
{

    private T _1;
    private T _2;
    private T _3;
    private T _4;
    private T _5;
    private T _6;

    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)6) throw new IndexOutOfRangeException();
            return Unsafe.Add(ref _1, index);
        }
        set
        {
            if ((uint)index >= (uint)6) throw new IndexOutOfRangeException();
            Unsafe.Add(ref _1, index) = value;
        }
    }

    public Span<T> AsSpan()
    {
        return new Span<T>(ref _1, 6);
    }
    
}
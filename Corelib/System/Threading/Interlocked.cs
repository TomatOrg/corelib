using System.Runtime.CompilerServices;

namespace System.Threading;

public static class Interlocked
{

    #region Add
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern int Add(ref int location1, int value);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern uint Add(ref uint location1, uint value);
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long Add(ref long location1, long value);
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern ulong Add(ref ulong location1, ulong value);

    #endregion
    
    #region And
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern int And(ref int location1, int value);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern uint And(ref uint location1, uint value);
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long And(ref long location1, long value);
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern ulong And(ref ulong location1, ulong value);

    #endregion

    #region Compare Exchange

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern int CompareExchange(ref int location1, int value, int comparand);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern uint CompareExchange(ref uint location1, uint value, uint comparand);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long CompareExchange(ref long location1, long value, long comparand);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern ulong CompareExchange(ref ulong location1, ulong value, ulong comparand);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern object CompareExchange(ref object location1, object value, object comparand);

    public static T CompareExchange<T>(ref T location1, T value, T comparand)
        where T : class
    {
        var obj = CompareExchange(ref Unsafe.As<T, object>(ref location1), value, comparand);
        return Unsafe.As<T>(obj);
    }

    #endregion

    #region Decrement

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern int Decrement(ref int location);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long Decrement(ref long location);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern uint Decrement(ref uint location);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern ulong Decrement(ref ulong location);

    #endregion
    
    #region Exchange

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern int Exchange(ref int location1, int value);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern uint Exchange(ref uint location1, uint value);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long Exchange(ref long location1, long value);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern ulong Exchange(ref ulong location1, ulong value);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern object Exchange(ref object location1, object value);

    public static T Exchange<T>(ref T location1, T value)
        where T : class
    {
        var obj = Exchange(ref Unsafe.As<T, object>(ref location1), value);
        return Unsafe.As<T>(obj);
    }

    #endregion

    #region Increment

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern int Increment(ref int location);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long Increment(ref long location);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern uint Increment(ref uint location);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern ulong Increment(ref ulong location);

    #endregion

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern void MemoryBarrier();
    
    #region Or
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern int Or(ref int location1, int value);

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern uint Or(ref uint location1, uint value);
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long Or(ref long location1, long value);
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern ulong Or(ref ulong location1, ulong value);

    #endregion

    #region Read
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long Read(ref long location);
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern ulong Read(ref ulong location);

    #endregion
    
}
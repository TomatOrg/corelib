using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System;

public readonly struct Double : ISpanFormattable, IComparable<double>, IEquatable<double>
{
    private readonly double m_value;

    //
    // Public Constants
    //
    public const double MinValue = -1.7976931348623157E+308;
    public const double MaxValue = 1.7976931348623157E+308;

    // Note Epsilon should be a double whose hex representation is 0x1
    // on little endian machines.
    public const double Epsilon = 4.9406564584124654E-324;
    public const double NegativeInfinity = (double)-1.0 / (double)(0.0);
    public const double PositiveInfinity = (double)1.0 / (double)(0.0);
    public const double NaN = (double)0.0 / (double)0.0;

    // We use this explicit definition to avoid the confusion between 0.0 and -0.0.
    internal const double NegativeZero = -0.0;

    //
    // Constants for manipulating the private bit-representation
    //

    internal const ulong SignMask = 0x8000_0000_0000_0000;
    internal const int SignShift = 63;

    internal const ulong ExponentMask = 0x7FF0_0000_0000_0000;
    internal const int ExponentShift = 52;
    internal const uint ShiftedExponentMask = (uint)(ExponentMask >> ExponentShift);

    internal const ulong SignificandMask = 0x000F_FFFF_FFFF_FFFF;

    internal const byte MinSign = 0;
    internal const byte MaxSign = 1;

    internal const ushort MinExponent = 0x0000;
    internal const ushort MaxExponent = 0x07FF;

    internal const ulong MinSignificand = 0x0000_0000_0000_0000;
    internal const ulong MaxSignificand = 0x000F_FFFF_FFFF_FFFF;

    /// <summary>Determines whether the specified value is finite (zero, subnormal, or normal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(double d)
    {
        long bits = BitConverter.DoubleToInt64Bits(d);
        return (bits & 0x7FFFFFFFFFFFFFFF) < 0x7FF0000000000000;
    }

    /// <summary>Determines whether the specified value is infinite.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInfinity(double d)
    {
        long bits = BitConverter.DoubleToInt64Bits(d);
        return (bits & 0x7FFFFFFFFFFFFFFF) == 0x7FF0000000000000;
    }
    
    /// <summary>Determines whether the specified value is NaN.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaN(double d)
    {
        // A NaN will never equal itself so this is an
        // easy and efficient way to check for NaN.

#pragma warning disable CS1718
        return d != d;
#pragma warning restore CS1718
    }
    
    /// <summary>Determines whether the specified value is negative.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(double d)
    {
        return BitConverter.DoubleToInt64Bits(d) < 0;
    }

    /// <summary>Determines whether the specified value is negative infinity.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegativeInfinity(double d)
    {
        return d == double.NegativeInfinity;
    }
    
    /// <summary>Determines whether the specified value is normal.</summary>
    // This is probably not worth inlining, it has branches and should be rarely called
    public static unsafe bool IsNormal(double d)
    {
        long bits = BitConverter.DoubleToInt64Bits(d);
        bits &= 0x7FFFFFFFFFFFFFFF;
        return (bits < 0x7FF0000000000000) && (bits != 0) && ((bits & 0x7FF0000000000000) != 0);
    }
    
    /// <summary>Determines whether the specified value is positive infinity.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositiveInfinity(double d)
    {
        return d == double.PositiveInfinity;
    }
    
    /// <summary>Determines whether the specified value is subnormal.</summary>
    // This is probably not worth inlining, it has branches and should be rarely called
    public static bool IsSubnormal(double d)
    {
        long bits = BitConverter.DoubleToInt64Bits(d);
        bits &= 0x7FFFFFFFFFFFFFFF;
        return (bits < 0x7FF0000000000000) && (bits != 0) && ((bits & 0x7FF0000000000000) == 0);
    }

    internal static int ExtractExponentFromBits(ulong bits)
    {
        return (int)(bits >> ExponentShift) & (int)ShiftedExponentMask;
    }

    internal static ulong ExtractSignificandFromBits(ulong bits)
    {
        return bits & SignificandMask;
    }

    public int CompareTo(double value)
    {
        if (m_value < value) return -1;
        if (m_value > value) return 1;
        if (m_value == value) return 0;

        // At least one of the values is NaN.
        if (IsNaN(m_value))
            return IsNaN(value) ? 0 : -1;
        else
            return 1;
    }

    // True if obj is another Double with the same value as the current instance.  This is
    // a method of object equality, that only returns true if obj is also a double.
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not double d)
        {
            return false;
        }
        double temp = d.m_value;
        // This code below is written this way for performance reasons i.e the != and == check is intentional.
        if (temp == m_value)
        {
            return true;
        }
        return IsNaN(temp) && IsNaN(m_value);
    }
    
    public bool Equals(double obj)
    {
        if (obj == m_value)
        {
            return true;
        }
        return IsNaN(obj) && IsNaN(m_value);
    }

    // The hashcode for a double is the absolute value of the integer representation
    // of that double.
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // 64-bit constants make the IL unusually large that makes the inliner to reject the method
    public override int GetHashCode()
    {
        long bits = Unsafe.As<double, long>(ref Unsafe.AsRef(in m_value));

        // Optimized check for IsNan() || IsZero()
        if (((bits - 1) & 0x7FFFFFFFFFFFFFFF) >= 0x7FF0000000000000)
        {
            // Ensure that all NaNs and both zeros have the same hash code
            bits &= 0x7FF0000000000000;
        }

        return unchecked((int)bits) ^ ((int)(bits >> 32));
    }
    
    public override string ToString()
    {
        return Number.FormatDouble(m_value, null, NumberFormatInfo.CurrentInfo);
    }

    public string ToString(string? format)
    {
        return Number.FormatDouble(m_value, format, NumberFormatInfo.CurrentInfo);
    }

    public string ToString(IFormatProvider? provider)
    {
        return Number.FormatDouble(m_value, null, NumberFormatInfo.GetInstance(provider));
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
        return Number.FormatDouble(m_value, format, NumberFormatInfo.GetInstance(provider));
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        return Number.TryFormatDouble(m_value, format, NumberFormatInfo.GetInstance(provider), destination, out charsWritten);
    }

    public static double Parse(string s)
    {
        if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
        return Number.ParseDouble(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo);
    }

    public static double Parse(string s, NumberStyles style)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
        if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
        return Number.ParseDouble(s, style, NumberFormatInfo.CurrentInfo);
    }

    public static double Parse(string s, IFormatProvider? provider)
    {
        if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
        return Number.ParseDouble(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance(provider));
    }

    public static double Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
        if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
        return Number.ParseDouble(s, style, NumberFormatInfo.GetInstance(provider));
    }

    // Parses a double from a String in the given style.  If
    // a NumberFormatInfo isn't specified, the current culture's
    // NumberFormatInfo is assumed.
    //
    // This method will not throw an OverflowException, but will return
    // PositiveInfinity or NegativeInfinity for a number that is too
    // large or too small.

    public static double Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands, IFormatProvider? provider = null)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
        return Number.ParseDouble(s, style, NumberFormatInfo.GetInstance(provider));
    }

    public static bool TryParse([NotNullWhen(true)] string? s, out double result)
    {
        if (s == null)
        {
            result = 0;
            return false;
        }

        return TryParse((ReadOnlySpan<char>)s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
    }

    public static bool TryParse(ReadOnlySpan<char> s, out double result)
    {
        return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out double result)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);

        if (s == null)
        {
            result = 0;
            return false;
        }

        return TryParse((ReadOnlySpan<char>)s, style, NumberFormatInfo.GetInstance(provider), out result);
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out double result)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
        return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
    }

    private static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out double result)
    {
        return Number.TryParseDouble(s, style, info, out result);
    }

}
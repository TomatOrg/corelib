using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System;

public readonly struct Single : ISpanFormattable, IComparable<float>, IEquatable<float>
{
    private readonly float m_value; // Do not rename (binary serialization)

    //
    // Public constants
    //
    public const float MinValue = (float)-3.40282346638528859e+38;
    public const float Epsilon = (float)1.4e-45;
    public const float MaxValue = (float)3.40282346638528859e+38;
    public const float PositiveInfinity = (float)1.0 / (float)0.0;
    public const float NegativeInfinity = (float)-1.0 / (float)0.0;
    public const float NaN = (float)0.0 / (float)0.0;

    // We use this explicit definition to avoid the confusion between 0.0 and -0.0.
    internal const float NegativeZero = (float)-0.0;

    //
    // Constants for manipulating the private bit-representation
    //

    internal const uint SignMask = 0x8000_0000;
    internal const int SignShift = 31;

    internal const uint ExponentMask = 0x7F80_0000;
    internal const int ExponentShift = 23;
    internal const uint ShiftedExponentMask = ExponentMask >> ExponentShift;

    internal const uint SignificandMask = 0x007F_FFFF;

    internal const byte MinSign = 0;
    internal const byte MaxSign = 1;

    internal const byte MinExponent = 0x00;
    internal const byte MaxExponent = 0xFF;

    internal const uint MinSignificand = 0x0000_0000;
    internal const uint MaxSignificand = 0x007F_FFFF;

    /// <summary>Determines whether the specified value is finite (zero, subnormal, or normal).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(float f)
    {
        int bits = BitConverter.SingleToInt32Bits(f);
        return (bits & 0x7FFFFFFF) < 0x7F800000;
    }

    /// <summary>Determines whether the specified value is infinite.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInfinity(float f)
    {
        int bits = BitConverter.SingleToInt32Bits(f);
        return (bits & 0x7FFFFFFF) == 0x7F800000;
    }

    /// <summary>Determines whether the specified value is NaN.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaN(float f)
    {
        // A NaN will never equal itself so this is an
        // easy and efficient way to check for NaN.

        #pragma warning disable CS1718
        return f != f;
        #pragma warning restore CS1718
    }

    /// <summary>Determines whether the specified value is negative.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(float f)
    {
        return BitConverter.SingleToInt32Bits(f) < 0;
    }

    /// <summary>Determines whether the specified value is negative infinity.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegativeInfinity(float f)
    {
        return f == float.NegativeInfinity;
    }

    /// <summary>Determines whether the specified value is normal.</summary>
    // This is probably not worth inlining, it has branches and should be rarely called
    public static bool IsNormal(float f)
    {
        int bits = BitConverter.SingleToInt32Bits(f);
        bits &= 0x7FFFFFFF;
        return (bits < 0x7F800000) && (bits != 0) && ((bits & 0x7F800000) != 0);
    }

    /// <summary>Determines whether the specified value is positive infinity.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositiveInfinity(float f)
    {
        return f == float.PositiveInfinity;
    }

    /// <summary>Determines whether the specified value is subnormal.</summary>
    // This is probably not worth inlining, it has branches and should be rarely called
    public static bool IsSubnormal(float f)
    {
        int bits = BitConverter.SingleToInt32Bits(f);
        bits &= 0x7FFFFFFF;
        return (bits < 0x7F800000) && (bits != 0) && ((bits & 0x7F800000) == 0);
    }

    internal static int ExtractExponentFromBits(uint bits)
    {
        return (int)(bits >> ExponentShift) & (int)ShiftedExponentMask;
    }

    internal static uint ExtractSignificandFromBits(uint bits)
    {
        return bits & SignificandMask;
    }
    
    public int CompareTo(float value)
    {
        if (m_value < value) return -1;
        if (m_value > value) return 1;
        if (m_value == value) return 0;

        // At least one of the values is NaN.
        if (IsNaN(m_value))
            return IsNaN(value) ? 0 : -1;
        else // f is NaN.
            return 1;
    }
    
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (!(obj is float))
        {
            return false;
        }
        float temp = ((float)obj).m_value;
        if (temp == m_value)
        {
            return true;
        }

        return IsNaN(temp) && IsNaN(m_value);
    }

    public bool Equals(float obj)
    {
        if (obj == m_value)
        {
            return true;
        }

        return IsNaN(obj) && IsNaN(m_value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        int bits = Unsafe.As<float, int>(ref Unsafe.AsRef(in m_value));

        // Optimized check for IsNan() || IsZero()
        if (((bits - 1) & 0x7FFFFFFF) >= 0x7F800000)
        {
            // Ensure that all NaNs and both zeros have the same hash code
            bits &= 0x7F800000;
        }

        return bits;
    }

    public override string ToString()
    {
        return Number.FormatSingle(m_value, null, NumberFormatInfo.CurrentInfo);
    }

    public string ToString(IFormatProvider? provider)
    {
        return Number.FormatSingle(m_value, null, NumberFormatInfo.GetInstance(provider));
    }

    public string ToString(string? format)
    {
        return Number.FormatSingle(m_value, format, NumberFormatInfo.CurrentInfo);
    }

    public string ToString(string? format, IFormatProvider? provider)
    {
        return Number.FormatSingle(m_value, format, NumberFormatInfo.GetInstance(provider));
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        return Number.TryFormatSingle(m_value, format, NumberFormatInfo.GetInstance(provider), destination, out charsWritten);
    }

    // Parses a float from a String in the given style.  If
    // a NumberFormatInfo isn't specified, the current culture's
    // NumberFormatInfo is assumed.
    //
    // This method will not throw an OverflowException, but will return
    // PositiveInfinity or NegativeInfinity for a number that is too
    // large or too small.
    //
    public static float Parse(string s)
    {
        if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
        return Number.ParseSingle(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo);
    }

    public static float Parse(string s, NumberStyles style)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
        if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
        return Number.ParseSingle(s, style, NumberFormatInfo.CurrentInfo);
    }

    public static float Parse(string s, IFormatProvider? provider)
    {
        if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
        return Number.ParseSingle(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.GetInstance(provider));
    }

    public static float Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
        if (s == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s);
        return Number.ParseSingle(s, style, NumberFormatInfo.GetInstance(provider));
    }

    public static float Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands, IFormatProvider? provider = null)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
        return Number.ParseSingle(s, style, NumberFormatInfo.GetInstance(provider));
    }

    public static bool TryParse([NotNullWhen(true)] string? s, out float result)
    {
        if (s == null)
        {
            result = 0;
            return false;
        }

        return TryParse((ReadOnlySpan<char>)s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
    }

    public static bool TryParse(ReadOnlySpan<char> s, out float result)
    {
        return TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.CurrentInfo, out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out float result)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);

        if (s == null)
        {
            result = 0;
            return false;
        }

        return TryParse((ReadOnlySpan<char>)s, style, NumberFormatInfo.GetInstance(provider), out result);
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out float result)
    {
        NumberFormatInfo.ValidateParseStyleFloatingPoint(style);
        return TryParse(s, style, NumberFormatInfo.GetInstance(provider), out result);
    }

    private static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out float result)
    {
        return Number.TryParseSingle(s, style, info, out result);
    }

}
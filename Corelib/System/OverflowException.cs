using System.Runtime.InteropServices;

namespace System;

[StructLayout(LayoutKind.Sequential)]
public class OverflowException : ArithmeticException
{

    internal const string NegateTwosCompNum = "Negating the minimum value of a twos complement number is invalid.";
    internal const string TimeSpanTooLong = "TimeSpan overflowed because the duration is too long.";
    internal const string Byte = "Value was either too large or too small for an unsigned byte.";
    internal const string UInt32 = "Value was either too large or too small for a UInt32.";
    internal const string UInt64 = "Value was either too large or too small for a UInt64.";
    internal const string Int32 = "Value was either too large or too small for an Int32.";
    internal const string Int64 = "Value was either too large or too small for an Int64.";
    
    internal const string Duration =
        " duration cannot be returned for TimeSpan.MinValue because the absolute value of TimeSpan.MinValue exceeds the value of TimeSpan.MaxValue.";
    
    public OverflowException()
        : base("Arithmetic operation resulted in an overflow.")
    {
    }

    public OverflowException(string message)
        : base(message)
    {
    }

    public OverflowException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
        
}
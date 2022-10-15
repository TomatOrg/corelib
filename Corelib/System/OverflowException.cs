using System.Runtime.InteropServices;

namespace System;

[StructLayout(LayoutKind.Sequential)]
public class OverflowException : ArithmeticException
{

    internal const string NegateTwosCompNum = "Negating the minimum value of a twos complement number is invalid.";
    internal const string TimeSpanTooLong = "TimeSpan overflowed because the duration is too long.";

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
namespace System;

public class ArgumentOutOfRangeException : ArgumentException
{

    internal const string StartIndex = "StartIndex cannot be less than zero.";
    internal const string IndexLength = "Index and length must refer to a location within the string.";
    internal const string Capacity = "Capacity exceeds maximum capacity.";
    internal const string SmallMaxCapacity = "MaxCapacity must be one or greater.";
    internal const string SmallCapacity = "capacity was less than the current size.";
    internal const string NegativeCapacity = "Capacity must be positive.";
    internal const string NegativeLength = "Length cannot be less than zero.";
    internal const string StartIndexLargerThanLength = "startIndex cannot be larger than length of string.";
    internal const string LengthGreaterThanCapacity = "The length cannot be greater than the capacity.";
    internal const string NegativeCount = "Count cannot be less than zero.";
    internal const string GenericPositive = "Value must be positive.";
    internal const string NegativeArgCount = "Argument count must not be negative.";
    internal const string NeedNonNegNum = "Non-negative number required.";
    internal const string IndexCountBuffer = "Index and count must refer to a location within the buffer.";
    internal const string IndexCount = "Index and count must refer to a location within the string.";
    internal const string NeedNonNegOrNegative1 = "Number must be either non-negative and less than or equal to Int32.MaxValue or -1.";
    internal const string Count = "Count must be positive and count must refer to a location within the string/array/collection.";
    internal const string AddValue = "Value to add was out of range.";
    internal const string LessEqualToIntegerMaxVal = "Argument must be less than or equal to 2^31 - 1 milliseconds.";
    internal const string TimeoutWrong = "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.";
    internal const string NeedPosNum = "Positive number required.";
    internal const string TimeoutTooLarge = "Time-out interval must be less than 2^32-2.";
    internal const string PeriodTooLarge = "Period must be less than 2^32-2.";
    internal const string StreamLength = "Stream length must be non-negative and less than 2^31 - 1 - origin.";
    internal const string GetByteCountOverflow = "Too many characters. The resulting number of bytes is larger than what can be returned as an int.";
    internal const string GetCharCountOverflow = "Too many bytes. The resulting number of chars is larger than what can be returned as an int.";
    internal const string MustBeNonNeg = "The number must be greater than or equal to zero.";
    internal const string OffsetLength = "Offset and length must refer to a position in the string.";
    internal const string InputTooLarge = "Input is too large to be processed.";
    
    internal const string InvalidHighSurrogate =
        "A valid high surrogate character is between 0xd800 and 0xdbff, inclusive.";

    internal const string InvalidLowSurrogate =
        "A valid low surrogate character is between 0xdc00 and 0xdfff, inclusive.";
    
    internal const string InvalidUTF32 =
        "A valid UTF32 value is between 0x000000 and 0x10ffff, inclusive, and should not include surrogate codepoint values (0x00d800 ~ 0x00dfff).";
    
    internal const string Index =
        "Index was out of range. Must be non-negative and less than the size of the collection.";
    
    public virtual object ActualValue { get; }

    public override string Message
    {
        get
        {
            var s = base.Message;
            if (ActualValue == null) 
                return s;

            var valueMessage = string.Concat("Actual value was ", ActualValue, ".");
            return s == null ? valueMessage : $"{s}\n{valueMessage}";
        }
    }

    public ArgumentOutOfRangeException()
        : base("Specified argument was out of the range of valid values.")
    {
    }

    public ArgumentOutOfRangeException(string paramName)
        : base(paramName)
    {
    }

    public ArgumentOutOfRangeException(string paramName, string message)
        : base(message, paramName)
    {
    }

    public ArgumentOutOfRangeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ArgumentOutOfRangeException(string paramName, object actualValue, string message)
        : base(message, paramName)
    {
        ActualValue = actualValue;
    }
}
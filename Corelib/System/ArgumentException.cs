namespace System;

public class ArgumentException : SystemException
{

    internal const string LongerThanSrcString = "Source string was not long enough. Check sourceIndex and count.";
    internal const string EmptyName = "Empty name is not legal.";
    internal const string StringZeroLength = "String cannot have zero length.";
    internal const string StringComparison = "The string comparison type passed in is currently not supported.";
    internal const string CannotBeNaN = "TimeSpan does not accept floating point Not-a-Number values.";
    internal const string InvalidFlag = "Value of flags is invalid.";
    internal const string EmptyTaskList = "The tasks argument contains no tasks.";
    internal const string NullTask = "The tasks argument included a null value.";
    internal const string DestinationTooShort = "Destination is too short.";
    internal const string ConversionOverflow = "Conversion buffer overflow.";
    internal const string InvalidCharSequenceNoIndex = "String contains invalid Unicode code points.";
    
    internal const string FallbackBufferNotEmpty =
        "Cannot change fallback when buffer is not empty. Previous Convert() call left data in the fallback buffer.";
    
    internal const string InvalidOffLen =
        "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
    
    internal const string DateTimeBadBinaryData =
        "The binary data must result in a DateTime with ticks between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.";
    
    internal const string InvalidGroupSize =
        "Every element in the value array should be between one and nine, except for the last element, which can be zero.";
    
    internal const string InvalidNativeDigitValue =
        "Each member of the NativeDigits array must be a single text element (one or more UTF16 code points) with a Unicode Nd (Number, Decimal Digit) property indicating it is a digit.";
    
    internal const string BufferNotFromPool =
        "The buffer is not associated with this pool and may not be returned to it.";
    
    internal const string OffsetOut =
        "Either offset did not refer to a position in the string, or there is an insufficient length of destination character array.";
    
    public string ParamName { get; }

    
    public override string Message
    {
        get
        {
            var s = base.Message ?? "Value does not fall within the expected range.";
            if (!string.IsNullOrEmpty(ParamName))
            {
                s = $"{s} (Parameter '{ParamName}')";
            }

            return s;
        }
    }

    public ArgumentException()
        : base("Value does not fall within the expected range.")
    {
    }

    public ArgumentException(string message)
        : base(message)
    {
    }

    public ArgumentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ArgumentException(string message, string paramName, Exception innerException)
        : base(message, innerException)
    {
        ParamName = paramName;
    }

    public ArgumentException(string message, string paramName)
        : base(message)
    {
        ParamName = paramName;
    }

}
namespace System;

public class FormatException : SystemException
{

    internal const string InvalidString = "Input string was not in a correct format.";
    internal const string IndexOutOfRange = "Index (zero based) must be greater than or equal to zero and less than the size of the argument list.";
    internal const string BadFormatSpecifier = "Format specifier was invalid.";
    internal const string NeedSingleChar = "String must be exactly one character long.";
    internal const string ExtraJunkAtEnd = "Additional non-parsable characters are at the end of the string.";
    internal const string GuidBrace = "Expected {0xdddddddd, etc}.";
    internal const string GuidDashes = "Dashes are in the wrong position for GUID parsing.";
    internal const string GuidEndBrace = "Could not find the ending brace.";
    internal const string GuidHexPrefix = "Expected 0x prefix.";
    internal const string GuidInvalidChar = "Guid string should only contain hexadecimal characters.";
    
    internal const string GuidComma =
        "Could not find a comma, or the length between the previous token and the comma was zero (i.e., '0x,'etc.).";

    internal const string GuidBraceAfterLastNumber =
        "Could not find a brace, or the length between the previous token and the brace was zero (i.e., '0x,'etc.).";
    
    internal const string GuidInvLen =
        "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).";
    
    internal const string InvalidGuidFormatSpecification =
        "Format string can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\".";
    
    public FormatException()
        : base("One of the identified items was in an invalid format.")
    {
    }

    public FormatException(string? message)
        : base(message)
    {
    }

    public FormatException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    
}
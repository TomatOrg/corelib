namespace System;

public class InvalidOperationException : SystemException
{
    
    internal const string EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";
    internal const string EnumNotStarted = "Enumeration has not started. Call MoveNext.";
    internal const string EnumEnded = "Enumeration already finished.";
    internal const string ReadOnly = "Instance is read-only.";
    internal const string TimeoutsNotSupported = "Timeouts are not supported on this stream.";

    internal const string CollectionCorrupted =
        "A prior operation on this collection was interrupted by an exception. Collection's state is no longer trusted.";
    
    public InvalidOperationException()
        : base("Operation is not valid due to the current state of the object.")
    {
    }

    public InvalidOperationException(string message)
        : base(message)
    {
    }

    public InvalidOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
    
}
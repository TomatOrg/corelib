namespace System.Threading;

public class SynchronizationLockException : SystemException
{

    internal const string DefaultMsg = "Object synchronization method was called from an unsynchronized block of code.";
    
    public SynchronizationLockException()
        : base(DefaultMsg)
    {
    }

    public SynchronizationLockException(string message)
        : base(message)
    {
    }

    public SynchronizationLockException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
    
}
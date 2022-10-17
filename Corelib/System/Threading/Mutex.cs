using TinyDotNet;

namespace System.Threading;

public sealed class Mutex : WaitHandle
{
    
    public Mutex(bool initiallyOwned = false)
    {
        _waitable = NativeHost.CreateWaitable(1);
        
        if (!initiallyOwned)
        {
            NativeHost.WaitableSend(_waitable, false);
        }
    }

    public void ReleaseMutex()
    {
        // TODO: somehow check out thread took the lock

        if (!NativeHost.WaitableSend(_waitable, false))
        {
            throw new ApplicationException(SynchronizationLockException.DefaultMsg);
        }
    }
    
}
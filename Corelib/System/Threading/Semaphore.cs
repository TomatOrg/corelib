using System.Runtime.CompilerServices;
using TinyDotNet;

namespace System.Threading;

public sealed class Semaphore : WaitHandle
{
    
    public Semaphore(int initialCount, int maximumCount)
    {
        if (initialCount < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCount), ArgumentOutOfRangeException.NeedNonNegNum);
        
        if (maximumCount < 1)
            throw new ArgumentOutOfRangeException(nameof(initialCount), ArgumentOutOfRangeException.NeedPosNum);
        
        if (initialCount > maximumCount)
            throw new ArgumentException("The initial count for the semaphore must be greater than or equal to zero and less than the maximum count.");

        // create it 
        _waitable = NativeHost.CreateWaitable(maximumCount);

        // fill it 
        if (initialCount > 0)
        {
            Release(initialCount);
        }
    }
    
    public void Release(int releaseCount = 1)
    {
        if (releaseCount < 1)
            throw new ArgumentOutOfRangeException(nameof(releaseCount), ArgumentOutOfRangeException.NeedNonNegNum);

        for (var i = 0; i < releaseCount; i++)
        {
            if (!NativeHost.WaitableSend(_waitable, false))
            {
                throw new SemaphoreFullException();
            }
        }
    }
    
}
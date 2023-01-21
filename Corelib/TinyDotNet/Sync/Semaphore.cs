namespace TinyDotNet.Sync;

internal struct Semaphore
{

    private new Mutex _mutex;
    private new Condition _condition;
    private int _value;

    internal Semaphore(int value)
    {
        _mutex = default;
        _condition = default;
        _value = value;
    }
    
    internal void Signal()
    {
        _mutex.Lock();
        _value++;
        _condition.NotifyOne();
        _mutex.Unlock();
    }

    internal bool Wait(int timeout)
    {
        var satisfied = true;
        
        //
        //     template<typename LockType, typename Functor>
        // bool waitUntilUnchecked(LockType& lock, const TimeWithDynamicClockType& timeout, const Functor& predicate) WTF_IGNORES_THREAD_SAFETY_ANALYSIS
        // {
        // while (!predicate()) {
        // if (!waitUntil(lock, timeout))
        // return predicate();
        //     }
        //     return true;
        // }
        //
        //
        
        _mutex.Lock();
        while (_value == 0)
        {
            // sleep for it, if true we did not get a timeout
            // TODO: this is kinda incorrect, we should
            // TODO: use a deadline instead...
            if (_condition.Wait(ref _mutex, timeout)) 
                continue;
         
            // we got a timeout, one last check and then exit
            satisfied = _value != 0;
            break;
        }
        _mutex.Unlock();
        
        if (satisfied)
            --_value;
        
        return satisfied;
    }
    
}
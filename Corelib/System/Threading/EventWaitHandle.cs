using TinyDotNet;

namespace System.Threading;

public class EventWaitHandle : WaitHandle
{

    internal EventResetMode _mode;
    
    public EventWaitHandle(bool initialState, EventResetMode mode)
    {
        _waitable = NativeHost.CreateWaitable(1);

        if (mode != EventResetMode.AutoReset && mode != EventResetMode.ManualReset)
        {
            throw new ArgumentException(ArgumentException.InvalidFlag, nameof(mode));
        }
        
        _mode = mode;
        if (initialState)
        {
            Set();
        }
        else
        {
            Reset();
        }
    }

    #region Auto Reset

    protected bool SetAutoReset()
    {
        return NativeHost.WaitableSend(_waitable, false);
    }

    protected bool ResetAutoReset()
    {
        return NativeHost.WaitableWait(_waitable, false) == 2;
    }

    #endregion

    #region Manual Reset
    
    protected bool SetManualReset()
    {
        // check the waitable is not closed already, if it is then there
        // is nothing to do
        if (NativeHost.WaitableWait(_waitable, false) == 1)
        {
            return false;
        }
        
        // close the waitable, waking all waiters
        NativeHost.WaitableClose(_waitable);

        return true;
    }

    protected bool ResetManualReset()
    {
        // check the waitable is closed, if not then there
        // is nothing  to do on the reset 
        if (NativeHost.WaitableWait(_waitable, false) != 1)
        {
            return false;
        }
        
        // re-open the waitable, allowing new people to listen on it 
        NativeHost.WaitableOpen(_waitable);
        
        return true;
    }


    #endregion
    
    public virtual bool Set()
    {
        return _mode == EventResetMode.AutoReset ? SetAutoReset() : SetManualReset();
    }

    public virtual bool Reset()
    {
        return _mode == EventResetMode.AutoReset ? ResetAutoReset() : ResetManualReset();
    }

}
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System;

[StructLayout(LayoutKind.Sequential)]
public class Exception
{

    internal string _message;
    private Exception _innerException;
        
    public virtual string Message => _message;
    public Exception InnerException => _innerException;

    public Exception()
    {
            
    }

    public Exception(string message)
    {
        _message = message;
    }

    public Exception(string message, Exception innerException)
    {
        _message = message;
        _innerException = innerException;
    }

    public virtual Exception GetBaseException()
    {
        var inner = InnerException;
        Exception back = null;
            
        while (inner != null)
        {
            back = inner;
            inner = inner.InnerException;
        }
            
        return back;
    }

    #region TODO: this

    
    internal readonly struct DispatchState
    {
    }

    internal DispatchState CaptureDispatchState()
    {
        return new DispatchState();
    }

    internal void RestoreDispatchState(in DispatchState state)
    {
    }
    
    
    [StackTraceHidden]
    internal void SetCurrentStackTrace()
    {
    }
    
    internal void SetRemoteStackTrace(string stackTrace)
    {
    }

    #endregion

}
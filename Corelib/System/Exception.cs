using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System;

[StructLayout(LayoutKind.Sequential)]
public class Exception
{

    private protected const string InnerExceptionPrefix = " ---> ";

    internal string _message;
    private Exception _innerException;

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

            
    public virtual string Message => _message ?? $"Exception of type '{GetClassName()}' was thrown.";

    private string GetClassName() => GetType().ToString();

    public Exception InnerException => _innerException;
    
    // Retrieves the lowest exception (inner most) for the given Exception.
    // This will traverse exceptions using the innerException property.
    public virtual Exception GetBaseException()
    {
        var inner = InnerException;
        Exception back = this;
            
        while (inner != null)
        {
            back = inner;
            inner = inner.InnerException;
        }
            
        return back;
    }
    
    // Returns the stack trace as a string.  If no stack trace is
    // available, null is returned.
    public virtual string? StackTrace => null;

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
    
    // this method is required so Object.GetType is not made virtual by the compiler
    // _Exception.GetType()
    public new Type GetType() => base.GetType();

    public override string ToString()
    {
        string className = GetClassName();
        string? message = Message;
        string innerExceptionString = _innerException?.ToString() ?? "";
        string endOfInnerExceptionResource = "--- End of inner exception stack trace ---";
        string? stackTrace = StackTrace;

        // Calculate result string length
        int length = className.Length;
        checked
        {
            if (!string.IsNullOrEmpty(message))
            {
                length += 2 + message.Length;
            }
            if (_innerException != null)
            {
                length += Environment.NewLineConst.Length + InnerExceptionPrefix.Length + innerExceptionString.Length + Environment.NewLineConst.Length + 3 + endOfInnerExceptionResource.Length;
            }
            if (stackTrace != null)
            {
                length += Environment.NewLineConst.Length + stackTrace.Length;
            }
        }

        // Create the string
        string result = string.FastAllocateString(length);
        Span<char> resultSpan = new Span<char>(ref result.GetRawStringData(), result.Length);

        // Fill it in
        Write(className, ref resultSpan);
        if (!string.IsNullOrEmpty(message))
        {
            Write(": ", ref resultSpan);
            Write(message, ref resultSpan);
        }
        if (_innerException != null)
        {
            Write(Environment.NewLineConst, ref resultSpan);
            Write(InnerExceptionPrefix, ref resultSpan);
            Write(innerExceptionString, ref resultSpan);
            Write(Environment.NewLineConst, ref resultSpan);
            Write("   ", ref resultSpan);
            Write(endOfInnerExceptionResource, ref resultSpan);
        }
        if (stackTrace != null)
        {
            Write(Environment.NewLineConst, ref resultSpan);
            Write(stackTrace, ref resultSpan);
        }
        Debug.Assert(resultSpan.Length == 0);

        // Return it
        return result;

        static void Write(string source, ref Span<char> dest)
        {
            source.CopyTo(dest);
            dest = dest.Slice(source.Length);
        }
    }

    
}
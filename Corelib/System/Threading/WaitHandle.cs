using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TinyDotNet;
using TinyDotNet.Sync;

namespace System.Threading;


public abstract class WaitHandle : IDisposable
{

    internal const int MaxWaitHandles = 64;
    
    // Successful wait on first object. When waiting for multiple objects the
    // return value is (WaitSuccess + waitIndex).
    internal const int WaitSuccess = 0;

    // The specified object is a mutex object that was not released by the
    // thread that owned the mutex object before the owning thread terminated.
    // When waiting for multiple objects the return value is (WaitAbandoned +
    // waitIndex).
    internal const int WaitAbandoned = 0x80;

    public const int WaitTimeout = 0x102;

    internal ulong _waitable = 0;

    // don't allow anyone but ourselves to inherit this
    internal WaitHandle()
    {
    }

    ~WaitHandle()
    {
        NativeHost.ReleaseWaitable(_waitable);
    }

    public void Close()
    {
        Dispose();
    }

    public void Dispose()
    {
        // TODO ???
    }
    
    internal static int ToTimeoutMilliseconds(TimeSpan timeout)
    {
        long timeoutMilliseconds = (long)timeout.TotalMilliseconds;
        if (timeoutMilliseconds < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), ArgumentOutOfRangeException.NeedNonNegOrNegative1);
        }
        if (timeoutMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), ArgumentOutOfRangeException.LessEqualToIntegerMaxVal);
        }
        return (int)timeoutMilliseconds;
    }
    
    public virtual bool WaitOne(int millisecondsTimeout)
    {
        if (millisecondsTimeout < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), ArgumentOutOfRangeException.NeedNonNegOrNegative1);
        }

        return WaitOneNoCheck(millisecondsTimeout);
    }
    
    private bool WaitOneNoCheck(int millisecondsTimeout)
    {
        Debug.Assert(millisecondsTimeout >= -1);
        
        // quick path for no timeout
        if (millisecondsTimeout == -1)
        {
            NativeHost.WaitableWait(_waitable, true);
            return true;
        }

        // otherwise we need to select on either a timeout or our handle
        var timeout = NativeHost.WaitableAfter(millisecondsTimeout * 1000);
        try
        {
            Span<ulong> waitables = stackalloc ulong[2];
            waitables[0] = timeout;
            waitables[1] = _waitable;

            return (int)NativeHost.WaitableSelect(waitables, 0, 2, true) == 1;
        }
        finally
        {
            NativeHost.ReleaseWaitable(timeout);
        }
    }
    
    private static int WaitMultiple(WaitHandle[] waitHandles, bool waitAll, int millisecondsTimeout)
    {
        if (waitHandles == null)
        {
            throw new ArgumentNullException(nameof(waitHandles), "The waitHandles parameter cannot be null.");
        }

        return WaitMultiple(new ReadOnlySpan<WaitHandle>(waitHandles), waitAll, millisecondsTimeout);
    }

    private static int WaitMultiple(ReadOnlySpan<WaitHandle> waitHandles, bool waitAll, int millisecondsTimeout)
    {
        if (waitHandles.Length == 0)
        {
            throw new ArgumentException("Waithandle array may not be empty.", nameof(waitHandles));
        }
        if (waitHandles.Length > MaxWaitHandles)
        {
            throw new NotSupportedException("The number of WaitHandles must be less than or equal to 64.");
        }
        if (millisecondsTimeout < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), ArgumentOutOfRangeException.NeedNonNegOrNegative1);
        }

        if (waitAll)
        {
            // need to wait on all of these
            if (millisecondsTimeout != -1)
            {
                var timeout = NativeHost.WaitableAfter(millisecondsTimeout * 1000);
                try
                {
                    // first is always the timeout
                    Span<ulong> handles = stackalloc ulong[2];
                    handles[0] = timeout;
                    
                    // no timeout, just wait for each of them 
                    foreach (var handle in waitHandles)
                    {
                        handles[1] = handle._waitable;
                        var index = (int)NativeHost.WaitableSelect(handles, 0, 2, true);

                        // if we got a timeout just return, no need to do anything else
                        if (index == 0)
                        {
                            return WaitTimeout;
                        }
                    }

                    // waited on all of them
                    return WaitSuccess;
                }
                finally
                {
                    NativeHost.ReleaseWaitable(timeout);
                }
            }
            else
            {
                // no timeout, just wait for each of them 
                foreach (var handle in waitHandles)
                {
                    NativeHost.WaitableWait(handle._waitable, true);
                }

                return WaitSuccess;
            }
        }
        else
        {
            // need to only wait on one, we will use select for that 
            
            // +1 for the timeout if any
            Span<ulong> handles = stackalloc ulong[MaxWaitHandles + 1];

            if (millisecondsTimeout != -1)
            {
                var timeout = NativeHost.WaitableAfter(millisecondsTimeout * 1000);
                try
                {
                    // the first is always the timeout 
                    handles[0] = timeout;
                    
                    // no timeout, just set everything up nicely 
                    for (var i = 0; i < waitHandles.Length; i++)
                    {
                        handles[i + 1] = waitHandles[i]._waitable;
                    }
                
                    // wait for one of them 
                    var index = (int)NativeHost.WaitableSelect(handles, 0, handles.Length, true);

                    // check if we got a timeout 
                    return index == 0 ? WaitTimeout : index - 1;
                }
                finally
                {
                    NativeHost.ReleaseWaitable(timeout);
                }
            }
            else
            {
                // no timeout, just set everything up nicely 
                for (var i = 0; i < waitHandles.Length; i++)
                {
                    handles[i] = waitHandles[i]._waitable;
                }
                
                return (int)NativeHost.WaitableSelect(handles, 0, handles.Length, true);
            }
        }
    }

    
    public bool WaitOne(TimeSpan timeout) => WaitOneNoCheck(ToTimeoutMilliseconds(timeout));
    public bool WaitOne() => WaitOneNoCheck(-1);
    public bool WaitOne(int millisecondsTimeout, bool exitContext) => WaitOne(millisecondsTimeout);
    public bool WaitOne(TimeSpan timeout, bool exitContext) => WaitOneNoCheck(ToTimeoutMilliseconds(timeout));

    public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout) =>
        WaitMultiple(waitHandles, true, millisecondsTimeout) != WaitTimeout;
    public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout) =>
        WaitMultiple(waitHandles, true, ToTimeoutMilliseconds(timeout)) != WaitTimeout;
    public static bool WaitAll(WaitHandle[] waitHandles) =>
        WaitMultiple(waitHandles, true, -1) != WaitTimeout;
    public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) =>
        WaitMultiple(waitHandles, true, millisecondsTimeout) != WaitTimeout;
    public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext) =>
        WaitMultiple(waitHandles, true, ToTimeoutMilliseconds(timeout)) != WaitTimeout;

    public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout) =>
        WaitMultiple(waitHandles, false, millisecondsTimeout);
    internal static int WaitAny(ReadOnlySpan<WaitHandle> waitHandles, int millisecondsTimeout) =>
        WaitMultiple(waitHandles, false, millisecondsTimeout);
    public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout) =>
        WaitMultiple(waitHandles, false, ToTimeoutMilliseconds(timeout));
    public static int WaitAny(WaitHandle[] waitHandles) =>
        WaitMultiple(waitHandles, false, -1);
    public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) =>
        WaitMultiple(waitHandles, false, millisecondsTimeout);
    public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout, bool exitContext) =>
        WaitMultiple(waitHandles, false, ToTimeoutMilliseconds(timeout));


}
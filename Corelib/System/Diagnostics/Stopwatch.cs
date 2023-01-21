using System.Runtime.CompilerServices;

namespace System.Diagnostics;

public class Stopwatch
{

    public static readonly long Frequency;
    public static readonly bool IsHighResolution = true;
    
    static Stopwatch()
    {
        Frequency = GetTscFrequency() * TimeSpan.TicksPerSecond;
    }

    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    private static extern long GetTscFrequency();

    private long _total = 0;
    private long _start = 0;

    public long ElapsedTicks
    {
        get
        {
            if (IsRunning)
            {
                return GetTimestamp() - _start + _total;
            }
            return _total;
        }
    }

    public TimeSpan Elapsed => new(ElapsedTicks);
    public long ElapsedMilliseconds => Elapsed.Milliseconds;
    
    public bool IsRunning { get; private set; }

    public Stopwatch()
    {
    }
    
    public void Reset()
    {
        Stop();
        _total = 0;
    }

    public void Restart()
    {
        Reset();
        Start();
    }
    
    public void Start()
    {
        IsRunning = true;
        _start = GetTimestamp();
    }
    
    public void Stop()
    {
        _total += GetTimestamp() - _start;
        IsRunning = false;
    }
    
    [MethodImpl(MethodCodeType = MethodCodeType.Native)]
    public static extern long GetTimestamp();

    public static Stopwatch StartNew()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        return stopwatch;
    }
    
}
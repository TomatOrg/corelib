using System.Runtime.CompilerServices;

namespace TinyDotNet;

/// <summary>
/// A collections of function that are required to be provided by
/// the native host and not by TinyDotNet itself
/// </summary>
internal static class NativeHost
{
    
    /// <summary>
    /// Get the current time from the RTC of the computer
    /// </summary>
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    public static extern void GetRtcTime(out int year, out int month, out int day, out int hour, out int minute, out int second);
    
}
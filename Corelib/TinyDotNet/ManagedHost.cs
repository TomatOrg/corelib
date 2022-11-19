using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TinyDotNet;

public static class ManagedHost
{

    #region Managed -> Managed

    /// <summary>
    /// Default the current time to be the time the binary was built, it is not
    /// perfect but it should be good enough :)
    /// </summary>
    internal static DateTime CurrentTimeBase = new DateTime(Builtin.CompileTime, DateTimeKind.Utc);

    /// <summary>
    /// Set the current time base, this also subtracts the current tick count
    /// so we can properly calculate the current time based on the tick count
    /// and the base time
    /// </summary>
    public static DateTime TimeBase
    {
        set => CurrentTimeBase = value.Subtract(TimeSpan.FromTicks(Environment.TickCount64));
    }

    #endregion

    #region Managed -> Native
    
    /// <summary>
    /// A callback passed to the native code so it can read from an assembly
    /// </summary>
    public delegate int ReadCallback(long offset, ref Memory<byte> output);

    /// <summary>
    /// A callback passed to native code so it can create a stream from the
    /// given an offset to the start of the file and a size that should
    /// be captured from the file
    /// </summary>
    public delegate Stream OpenStreamCallback(long offset, long size);
    
    [StructLayout(LayoutKind.Sequential)]
    public struct AssemblyFile
    {
        public ReadCallback Read;
        public OpenStreamCallback OpenStream;
    }

    /// <summary>
    /// Called by the native code in order to open an assembly by its name
    /// and give back a struct with functions that can be used to handle
    /// the file safely
    /// </summary>
    public delegate bool OpenAssembly(string name, out AssemblyFile file);

    /// <summary>
    /// Call to set the open assembly callback
    /// </summary>
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Native)]
    public static extern void SetOpenAssemblyCallback(OpenAssembly callback);

    #endregion

}
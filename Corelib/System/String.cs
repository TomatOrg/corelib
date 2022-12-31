// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System;

// The String class represents a static string of characters.  Many of
// the string methods perform some type of transformation on the current
// instance and return the result as a new string.  As with arrays, character
// positions (indices) are zero-based.
[StructLayout(LayoutKind.Sequential)]
public partial class String : IEnumerable<char>, IComparable<string?>, IEquatable<string?>
{
    
    /// <summary>Maximum length allowed for a string.</summary>
    internal const int MaxLength = int.MaxValue / sizeof(char);

    public static readonly string Empty = "";

    private readonly int _stringLength;

    private class RawData
    {
        internal char Data;
    }
    
    private ref char _firstChar => ref Unsafe.AddByteOffset(ref Unsafe.As<RawData>(this).Data, 16 + 4);

    public int Length => _stringLength;

    [IndexerName("Chars")]
    public char this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException();
            return Unsafe.Add(ref _firstChar, index);
        }
    }
    
    /// <summary>
    /// Returns a reference to the first element of the String. If the string is null, an access will throw a NullReferenceException.
    /// </summary>
    public ref readonly char GetPinnableReference() => ref _firstChar;

    internal ref char GetRawStringData() => ref _firstChar;

    private String(int length)
    {
        _stringLength = length;
    }

    internal static string FastAllocateString(int length)
    {
        return new string(length);
    }

    internal static unsafe string CreateStringFromEncoding(byte* bytes, int byteLength, Encoding encoding)
    {
        Debug.Assert(bytes != null);
        Debug.Assert(byteLength >= 0);

        // Get our string length
        int stringLength = encoding.GetCharCount(bytes, byteLength);
        Debug.Assert(stringLength >= 0, "stringLength >= 0");

        // They gave us an empty string if they needed one
        // 0 bytelength might be possible if there's something in an encoder
        if (stringLength == 0)
            return Empty;

        string s = FastAllocateString(stringLength);
        fixed (char* pTempChars = &s._firstChar)
        {
            int doubleCheck = encoding.GetChars(bytes, byteLength, pTempChars, stringLength);
            Debug.Assert(stringLength == doubleCheck,
                "Expected encoding.GetChars to return same length as encoding.GetCharCount");
        }

        return s;
    }
    
    public String(char[] chars)
    {
        _stringLength = chars.Length;
        
        var span = new Span<char>(ref GetRawStringData(), Length);
        chars.AsSpan().CopyTo(span);
    }

    public String(char[] chars, int startIndex, int length)
    {
        _stringLength = length;

        var span = new Span<char>(ref GetRawStringData(), Length);
        chars.AsSpan(startIndex, length).CopyTo(span);
    }
    
    public String(ReadOnlySpan<char> chars)
    {
        _stringLength = chars.Length;
        
        var span = new Span<char>(ref GetRawStringData(), Length);
        chars.CopyTo(span);
    }

    public CharEnumerator GetEnumerator()
    {
        return new CharEnumerator(this);
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    IEnumerator<char> IEnumerable<char>.GetEnumerator()
    {
        return new CharEnumerator(this);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int wcslen(char* ptr)
    {
        // IndexOf processes memory in aligned chunks, and thus it won't crash even if it accesses memory beyond the null terminator.
        // This IndexOf behavior is an implementation detail of the runtime and callers outside System.Private.CoreLib must not depend on it.
        int length = SpanHelpers.IndexOf(ref *ptr, '\0', int.MaxValue);
        if (length < 0)
        {
            ThrowMustBeNullTerminatedString();
        }

        return length;
    }
    
    [DoesNotReturn]
    private static void ThrowMustBeNullTerminatedString()
    {
        throw new ArgumentException("The string must be null-terminated.");
    }

    public static string Create<TState>(int length, TState state, Buffers.Action.SpanAction<char, TState> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (length <= 0)
        {
            if (length == 0)
                return Empty;
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        string result = FastAllocateString(length);
        action(new Span<char>(ref result.GetRawStringData(), length), state);
        return result;
    }
    
    /// <summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="handler">The interpolated string.</param>
    /// <returns>The string that results for formatting the interpolated string using the specified format provider.</returns>
    public static string Create(IFormatProvider? provider, [InterpolatedStringHandlerArgument("provider")] ref DefaultInterpolatedStringHandler handler) =>
        handler.ToStringAndClear();

    /// <summary>Creates a new string by using the specified provider to control the formatting of the specified interpolated string.</summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="initialBuffer">The initial buffer that may be used as temporary space as part of the formatting operation. The contents of this buffer may be overwritten.</param>
    /// <param name="handler">The interpolated string.</param>
    /// <returns>The string that results for formatting the interpolated string using the specified format provider.</returns>
    public static string Create(IFormatProvider? provider, Span<char> initialBuffer, [InterpolatedStringHandlerArgument("provider", "initialBuffer")] ref DefaultInterpolatedStringHandler handler) =>
        handler.ToStringAndClear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<char>(string? value) =>
        value != null ? new ReadOnlySpan<char>(ref value.GetRawStringData(), value.Length) : default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetSpan(int startIndex, int count, out ReadOnlySpan<char> slice)
    {
        // See comment in Span<T>.Slice for how this works.
        if ((ulong)(uint)startIndex + (ulong)(uint)count > (ulong)(uint)Length)
        {
            slice = default;
            return false;
        }
        
        slice = new ReadOnlySpan<char>(ref Unsafe.Add(ref _firstChar, (nint)(uint)startIndex /* force zero-extension */), count);
        return true;
    }

    // This is only intended to be used by char.ToString.
    // It is necessary to put the code in this class instead of Char, since _firstChar is a private member.
    // Making _firstChar internal would be dangerous since it would make it much easier to break String's immutability.
    internal static string CreateFromChar(char c)
    {
        string result = FastAllocateString(1);
        result._firstChar = c;
        return result;
    }
    
    // Converts a substring of this string to an array of characters.  Copies the
    // characters of this string beginning at position sourceIndex and ending at
    // sourceIndex + count - 1 to the character array buffer, beginning
    // at destinationIndex.
    //
    public unsafe void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), ArgumentOutOfRangeException.NegativeCount);
        if (sourceIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(sourceIndex), ArgumentOutOfRangeException.Index);
        if (count > Length - sourceIndex)
            throw new ArgumentOutOfRangeException(nameof(sourceIndex), ArgumentOutOfRangeException.IndexCount);
        if (destinationIndex > destination.Length - count || destinationIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(destinationIndex), ArgumentOutOfRangeException.IndexCount);

        Buffer.Memmove(
            destination: ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(destination), destinationIndex),
            source: ref Unsafe.Add(ref _firstChar, sourceIndex),
            elementCount: (uint)count);
    }
    
    /// <summary>Copies the contents of this string into the destination span.</summary>
    /// <param name="destination">The span into which to copy this string's contents.</param>
    /// <exception cref="System.ArgumentException">The destination span is shorter than the source string.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<char> destination)
    {
        if ((uint)Length <= (uint)destination.Length)
        {
            Buffer.Memmove(ref destination._pointer.Value, ref _firstChar, (uint)Length);
        }
        else
        {
            ThrowHelper.ThrowArgumentException_DestinationTooShort();
        }
    }
    
    /// <summary>Copies the contents of this string into the destination span.</summary>
    /// <param name="destination">The span into which to copy this string's contents.</param>
    /// <returns>true if the data was copied; false if the destination was too short to fit the contents of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryCopyTo(Span<char> destination)
    {
        return this.AsSpan().TryCopyTo(destination);
    }

    // Returns the entire string as an array of characters.
    public char[] ToCharArray()
    {
        return this.AsSpan().ToArray();
    }
    
    // Returns a substring of this string as an array of characters.
    //
    public char[] ToCharArray(int startIndex, int length)
    {
        return this.AsSpan(startIndex, length).ToArray();
    }
    
    public override string ToString()
    {
        return this;
    }
    
    public static bool IsNullOrEmpty([NotNullWhen(false)] string? value)
    {
        return value == null || 0 == value.Length;
    }
    
}
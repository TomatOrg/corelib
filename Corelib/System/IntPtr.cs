// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable SA1121 // explicitly using type aliases instead of built-in types
using nint_t = System.Int64;

namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct IntPtr : IEquatable<nint>, IComparable<nint>, ISpanFormattable
    {
        // WARNING: We allow diagnostic tools to directly inspect this member (_value).
        // See https://github.com/dotnet/corert/blob/master/Documentation/design-docs/diagnostics/diagnostics-tools-contract.md for more details.
        // Please do not change the type, the name, or the semantic usage of this member without understanding the implication for tools.
        // Get in touch with the diagnostics team if you have questions.
        private readonly unsafe void* _value; // Do not rename (binary serialization)

        public static readonly IntPtr Zero;

        public unsafe IntPtr(int value)
        {
            _value = (void*)value;
        }

        public unsafe IntPtr(long value)
        {
            _value = (void*)value;
        }

        public unsafe IntPtr(void* value)
        {
            _value = value;
        }

        public override unsafe bool Equals([NotNullWhen(true)] object? obj) =>
            obj is IntPtr other &&
            _value == other._value;

        public override unsafe int GetHashCode()
        {
            long l = (long)_value;
            return unchecked((int)l) ^ (int)(l >> 32);
        }

        public unsafe int ToInt32()
        {
            long l = (long)_value;
            return checked((int)l);
        }

        public unsafe long ToInt64() =>
            (nint)_value;

        public static unsafe explicit operator IntPtr(int value) =>
            new IntPtr(value);

        public static unsafe explicit operator IntPtr(long value) =>
            new IntPtr(value);

        public static unsafe explicit operator IntPtr(void* value) =>
            new IntPtr(value);

        public static unsafe explicit operator void*(IntPtr value) =>
            value._value;

        public static unsafe explicit operator int(IntPtr value)
        {
            long l = (long)value._value;
            return checked((int)l);
        }

        public static unsafe explicit operator long(IntPtr value) =>
            (nint)value._value;

        public static unsafe bool operator ==(IntPtr value1, IntPtr value2) =>
            value1._value == value2._value;

        public static unsafe bool operator !=(IntPtr value1, IntPtr value2) =>
            value1._value != value2._value;

        public static IntPtr Add(IntPtr pointer, int offset) =>
            pointer + offset;

        public static unsafe IntPtr operator +(IntPtr pointer, int offset) =>
            (nint)pointer._value + offset;

        public static IntPtr Subtract(IntPtr pointer, int offset) =>
            pointer - offset;

        public static unsafe IntPtr operator -(IntPtr pointer, int offset) =>
            (nint)pointer._value - offset;

        public static int Size
        {
            get => sizeof(nint_t);
        }

        public unsafe void* ToPointer() => _value;

        public static IntPtr MaxValue
        {
            get => (IntPtr)nint_t.MaxValue;
        }

        public static IntPtr MinValue
        {
            get => (IntPtr)nint_t.MinValue;
        }

        public unsafe int CompareTo(IntPtr value) => ((nint_t)_value).CompareTo((nint_t)value);

        public unsafe bool Equals(IntPtr other) => (nint_t)_value == (nint_t)other;

        public unsafe override string ToString() => ((nint_t)_value).ToString();
        public unsafe string ToString(string? format) => ((nint_t)_value).ToString(format);
        public unsafe string ToString(IFormatProvider? provider) => ((nint_t)_value).ToString(provider);
        public unsafe string ToString(string? format, IFormatProvider? provider) => ((nint_t)_value).ToString(format, provider);

        public unsafe bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) =>
            ((nint_t)_value).TryFormat(destination, out charsWritten, format, provider);

        public static IntPtr Parse(string s) => (IntPtr)nint_t.Parse(s);
        public static IntPtr Parse(string s, NumberStyles style) => (IntPtr)nint_t.Parse(s, style);
        public static IntPtr Parse(string s, IFormatProvider? provider) => (IntPtr)nint_t.Parse(s, provider);
        public static IntPtr Parse(string s, NumberStyles style, IFormatProvider? provider) => (IntPtr)nint_t.Parse(s, style, provider);
        public static IntPtr Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null) => (IntPtr)nint_t.Parse(s, style, provider);

        public static bool TryParse([NotNullWhen(true)] string? s, out IntPtr result)
        {
            result = Zero;
            return nint_t.TryParse(s, out Unsafe.As<IntPtr, nint_t>(ref result));
        }

        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out IntPtr result)
        {
            result = Zero;
            return nint_t.TryParse(s, style, provider, out Unsafe.As<IntPtr, nint_t>(ref result));
        }

        public static bool TryParse(ReadOnlySpan<char> s, out IntPtr result)
        {
            result = Zero;
            return nint_t.TryParse(s, out Unsafe.As<IntPtr, nint_t>(ref result));
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out IntPtr result)
        {
            result = Zero;
            return nint_t.TryParse(s, style, provider, out Unsafe.As<IntPtr, nint_t>(ref result));
        }

    }
}

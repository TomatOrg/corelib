// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable SA1121 // explicitly using type aliases instead of built-in types
using nuint_t = System.UInt64;

namespace System
{

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct UIntPtr : IEquatable<nuint>, IComparable<nuint>, ISpanFormattable
    {
        private readonly unsafe void* _value; // Do not rename (binary serialization)

        public static readonly UIntPtr Zero;

        public unsafe UIntPtr(uint value)
        {
            _value = (void*)value;
        }

        public unsafe UIntPtr(ulong value)
        {
            _value = (void*)value;
        }

        public unsafe UIntPtr(void* value)
        {
            _value = value;
        }

        public override unsafe bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is UIntPtr)
            {
                return _value == ((UIntPtr)obj)._value;
            }
            return false;
        }

        public override unsafe int GetHashCode()
        {
            ulong l = (ulong)_value;
            return unchecked((int)l) ^ (int)(l >> 32);
        }

        public unsafe uint ToUInt32()
        {
            return checked((uint)_value);
        }

        public unsafe ulong ToUInt64() => (ulong)_value;

        public static explicit operator UIntPtr(uint value) =>
            new UIntPtr(value);

        public static explicit operator UIntPtr(ulong value) =>
            new UIntPtr(value);

        public static unsafe explicit operator UIntPtr(void* value) =>
            new UIntPtr(value);

        public static unsafe explicit operator void*(UIntPtr value) =>
            value._value;

        public static unsafe explicit operator uint(UIntPtr value) =>
            checked((uint)value._value);

        public static unsafe explicit operator ulong(UIntPtr value) =>
            (ulong)value._value;

        public static unsafe bool operator ==(UIntPtr value1, UIntPtr value2) =>
            value1._value == value2._value;

        public static unsafe bool operator !=(UIntPtr value1, UIntPtr value2) =>
            value1._value != value2._value;

        public static UIntPtr Add(UIntPtr pointer, int offset) =>
            pointer + offset;

        public static unsafe UIntPtr operator +(UIntPtr pointer, int offset) =>
            (nuint)pointer._value + (nuint)offset;

        public static UIntPtr Subtract(UIntPtr pointer, int offset) =>
            pointer - offset;

        public static unsafe UIntPtr operator -(UIntPtr pointer, int offset) =>
            (nuint)pointer._value - (nuint)offset;

        public static int Size => sizeof(nuint_t);

        public unsafe void* ToPointer() => _value;

        public static UIntPtr MaxValue => (UIntPtr)nuint_t.MaxValue;

        public static UIntPtr MinValue => (UIntPtr)nuint_t.MinValue;

        public unsafe int CompareTo(UIntPtr value) => ((nuint_t)_value).CompareTo((nuint_t)value);

        public unsafe bool Equals(UIntPtr other) => (nuint)_value == (nuint)other;

        public unsafe override string ToString() => ((nuint_t)_value).ToString();
        public unsafe string ToString(string? format) => ((nuint_t)_value).ToString(format);
        public unsafe string ToString(IFormatProvider? provider) => ((nuint_t)_value).ToString(provider);
        public unsafe string ToString(string? format, IFormatProvider? provider) => ((nuint_t)_value).ToString(format, provider);

        public unsafe bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) =>
            ((nuint_t)_value).TryFormat(destination, out charsWritten, format, provider);

        public static UIntPtr Parse(string s) => (UIntPtr)nuint_t.Parse(s);
        public static UIntPtr Parse(string s, NumberStyles style) => (UIntPtr)nuint_t.Parse(s, style);
        public static UIntPtr Parse(string s, IFormatProvider? provider) => (UIntPtr)nuint_t.Parse(s, provider);
        public static UIntPtr Parse(string s, NumberStyles style, IFormatProvider? provider) => (UIntPtr)nuint_t.Parse(s, style, provider);
        public static UIntPtr Parse(ReadOnlySpan<char> s, NumberStyles style = NumberStyles.Integer, IFormatProvider? provider = null) => (UIntPtr)nuint_t.Parse(s, style, provider);

        public static bool TryParse([NotNullWhen(true)] string? s, out UIntPtr result)
        {
            result = Zero;
            return nuint_t.TryParse(s, out Unsafe.As<UIntPtr, nuint_t>(ref result));
        }

        public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out UIntPtr result)
        {
            result = Zero;
            return nuint_t.TryParse(s, style, provider, out Unsafe.As<UIntPtr, nuint_t>(ref result));
        }

        public static bool TryParse(ReadOnlySpan<char> s, out UIntPtr result)
        {
            result = Zero;
            return nuint_t.TryParse(s, out Unsafe.As<UIntPtr, nuint_t>(ref result));
        }

        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out UIntPtr result)
        {
            result = Zero;
            return nuint_t.TryParse(s, style, provider, out Unsafe.As<UIntPtr, nuint_t>(ref result));
        }

    }
}

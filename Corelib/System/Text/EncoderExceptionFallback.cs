// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace System.Text
{
    public sealed class EncoderExceptionFallback : EncoderFallback
    {
        internal static readonly EncoderExceptionFallback s_default = new EncoderExceptionFallback();

        // Construction
        public EncoderExceptionFallback()
        {
        }

        public override EncoderFallbackBuffer CreateFallbackBuffer() =>
            new EncoderExceptionFallbackBuffer();

        // Maximum number of characters that this instance of this fallback could return
        public override int MaxCharCount => 0;

        public override bool Equals([NotNullWhen(true)] object? value) =>
            value is EncoderExceptionFallback;

        public override int GetHashCode() => 654;
    }


    public sealed class EncoderExceptionFallbackBuffer : EncoderFallbackBuffer
    {
        public EncoderExceptionFallbackBuffer() { }
        public override bool Fallback(char charUnknown, int index)
        {
            // Fall back our char
            throw new EncoderFallbackException(
                $"Unable to translate Unicode character \\u{(int)charUnknown:X4} at index {index} to specified code page.",
                charUnknown, index);
        }

        public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
        {
            if (!char.IsHighSurrogate(charUnknownHigh))
            {
                throw new ArgumentOutOfRangeException(nameof(charUnknownHigh),
                    "Valid values are between 0xD800 and 0xDBFF, inclusive.");
            }
            if (!char.IsLowSurrogate(charUnknownLow))
            {
                throw new ArgumentOutOfRangeException(nameof(charUnknownLow),
                    "Valid values are between 0xDC00 and 0xDFFF, inclusive.");
            }

            int iTemp = char.ConvertToUtf32(charUnknownHigh, charUnknownLow);

            // Fall back our char
            throw new EncoderFallbackException(
                $"Unable to translate Unicode character \\u{iTemp:X4} at index {index} to specified code page.",
                charUnknownHigh, charUnknownLow, index);
        }

        public override char GetNextChar() => (char)0;

        // Exception fallback doesn't have anywhere to back up to.
        public override bool MovePrevious() => false;

        // Exceptions are always empty
        public override int Remaining => 0;
    }

    public sealed class EncoderFallbackException : ArgumentException
    {
        private readonly char _charUnknown;
        private readonly char _charUnknownHigh;
        private readonly char _charUnknownLow;
        private readonly int _index;

        public EncoderFallbackException()
            : base("Value does not fall within the expected range.")
        {
        }

        public EncoderFallbackException(string? message)
            : base(message)
        {
        }

        public EncoderFallbackException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        internal EncoderFallbackException(
            string? message, char charUnknown, int index) : base(message)
        {
            _charUnknown = charUnknown;
            _index = index;
        }

        internal EncoderFallbackException(
            string message, char charUnknownHigh, char charUnknownLow, int index) : base(message)
        {
            if (!char.IsHighSurrogate(charUnknownHigh))
            {
                throw new ArgumentOutOfRangeException(nameof(charUnknownHigh),
                    "Valid values are between 0xD800 and 0xDBFF, inclusive.");
            }
            if (!char.IsLowSurrogate(charUnknownLow))
            {
                throw new ArgumentOutOfRangeException(nameof(CharUnknownLow),
                    "Valid values are between 0xDC00 and 0xDFFF, inclusive.");
            }

            _charUnknownHigh = charUnknownHigh;
            _charUnknownLow = charUnknownLow;
            _index = index;
        }

        public char CharUnknown => _charUnknown;

        public char CharUnknownHigh => _charUnknownHigh;

        public char CharUnknownLow => _charUnknownLow;

        public int Index => _index;

        // Return true if the unknown character is a surrogate pair.
        public bool IsUnknownSurrogate() => _charUnknownHigh != '\0';
    }
}

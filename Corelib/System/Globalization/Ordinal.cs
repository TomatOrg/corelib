// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text.Unicode;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
    internal static partial class Ordinal
    {
        internal static int CompareStringIgnoreCase(ref char strA, int lengthA, ref char strB, int lengthB)
        {
            int length = Math.Min(lengthA, lengthB);
            int range = length;

            ref char charA = ref strA;
            ref char charB = ref strB;

            char maxChar = (char)0x7F;

            while (length != 0 && charA <= maxChar && charB <= maxChar)
            {
                // Ordinal equals or lowercase equals if the result ends up in the a-z range
                if (charA == charB ||
                    ((charA | 0x20) == (charB | 0x20) &&
                        (uint)((charA | 0x20) - 'a') <= (uint)('z' - 'a')))
                {
                    length--;
                    charA = ref Unsafe.Add(ref charA, 1);
                    charB = ref Unsafe.Add(ref charB, 1);
                }
                else
                {
                    int currentA = charA;
                    int currentB = charB;

                    // Uppercase both chars if needed
                    if ((uint)(charA - 'a') <= 'z' - 'a')
                    {
                        currentA -= 0x20;
                    }
                    if ((uint)(charB - 'a') <= 'z' - 'a')
                    {
                        currentB -= 0x20;
                    }

                    // Return the (case-insensitive) difference between them.
                    return currentA - currentB;
                }
            }

            if (length == 0)
            {
                return lengthA - lengthB;
            }

            range -= length;

            return CompareStringIgnoreCaseNonAscii(ref charA, lengthA - range, ref charB, lengthB - range);
        }

        internal static int CompareStringIgnoreCaseNonAscii(ref char strA, int lengthA, ref char strB, int lengthB)
        {
            return OrdinalCasing.CompareStringIgnoreCase(ref strA, lengthA, ref strB, lengthB);
        }

        internal static bool EqualsIgnoreCase(ref char charA, ref char charB, int length)
        {
            nint byteOffset = 0;

            // Read 4 chars (64 bits) at a time from each string
            while ((uint)length >= 4)
            {
                ulong valueA = Unsafe.ReadUnaligned<ulong>(ref Unsafe.As<char, byte>(ref Unsafe.AddByteOffset(ref charA, byteOffset)));
                ulong valueB = Unsafe.ReadUnaligned<ulong>(ref Unsafe.As<char, byte>(ref Unsafe.AddByteOffset(ref charB, byteOffset)));

                // A 32-bit test - even with the bit-twiddling here - is more efficient than a 64-bit test.
                ulong temp = valueA | valueB;
                if (!Utf16Utility.AllCharsInUInt32AreAscii((uint)temp | (uint)(temp >> 32)))
                {
                    goto NonAscii; // one of the inputs contains non-ASCII data
                }

                // Generally, the caller has likely performed a first-pass check that the input strings
                // are likely equal. Consider a dictionary which computes the hash code of its key before
                // performing a proper deep equality check of the string contents. We want to optimize for
                // the case where the equality check is likely to succeed, which means that we want to avoid
                // branching within this loop unless we're about to exit the loop, either due to failure or
                // due to us running out of input data.

                if (!Utf16Utility.UInt64OrdinalIgnoreCaseAscii(valueA, valueB))
                {
                    return false;
                }

                byteOffset += 8;
                length -= 4;
            }

            // Read 2 chars (32 bits) at a time from each string
            if ((uint)length >= 2)
            {
                uint valueA = Unsafe.ReadUnaligned<uint>(ref Unsafe.As<char, byte>(ref Unsafe.AddByteOffset(ref charA, byteOffset)));
                uint valueB = Unsafe.ReadUnaligned<uint>(ref Unsafe.As<char, byte>(ref Unsafe.AddByteOffset(ref charB, byteOffset)));

                if (!Utf16Utility.AllCharsInUInt32AreAscii(valueA | valueB))
                {
                    goto NonAscii; // one of the inputs contains non-ASCII data
                }

                // Generally, the caller has likely performed a first-pass check that the input strings
                // are likely equal. Consider a dictionary which computes the hash code of its key before
                // performing a proper deep equality check of the string contents. We want to optimize for
                // the case where the equality check is likely to succeed, which means that we want to avoid
                // branching within this loop unless we're about to exit the loop, either due to failure or
                // due to us running out of input data.

                if (!Utf16Utility.UInt32OrdinalIgnoreCaseAscii(valueA, valueB))
                {
                    return false;
                }

                byteOffset += 4;
                length -= 2;
            }

            if (length != 0)
            {
                Debug.Assert(length == 1);

                uint valueA = Unsafe.AddByteOffset(ref charA, byteOffset);
                uint valueB = Unsafe.AddByteOffset(ref charB, byteOffset);

                if ((valueA | valueB) > 0x7Fu)
                {
                    goto NonAscii; // one of the inputs contains non-ASCII data
                }

                if (valueA == valueB)
                {
                    return true; // exact match
                }

                valueA |= 0x20u;
                if ((uint)(valueA - 'a') > (uint)('z' - 'a'))
                {
                    return false; // not exact match, and first input isn't in [A-Za-z]
                }

                // The ternary operator below seems redundant but helps RyuJIT generate more optimal code.
                // See https://github.com/dotnet/runtime/issues/4207.
                return (valueA == (valueB | 0x20u)) ? true : false;
            }

            Debug.Assert(length == 0);
            return true;

        NonAscii:
            // The non-ASCII case is factored out into its own helper method so that the JIT
            // doesn't need to emit a complex prolog for its caller (this method).
            return CompareStringIgnoreCase(ref Unsafe.AddByteOffset(ref charA, byteOffset), length, ref Unsafe.AddByteOffset(ref charB, byteOffset), length) == 0;
        }

        internal static unsafe int IndexOf(string source, string value, int startIndex, int count, bool ignoreCase)
        {
            if (source == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (value == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }

            if (!source.TryGetSpan(startIndex, count, out ReadOnlySpan<char> sourceSpan))
            {
                // Bounds check failed - figure out exactly what went wrong so that we can
                // surface the correct argument exception.

                if ((uint)startIndex > (uint)source.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
                }
                else
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
                }
            }

            int result = ignoreCase ? IndexOfOrdinalIgnoreCase(sourceSpan, value) : sourceSpan.IndexOf(value);

            return result >= 0 ? result + startIndex : result;
        }

        internal static int IndexOfOrdinalIgnoreCase(ReadOnlySpan<char> source, ReadOnlySpan<char> value)
        {
            if (value.Length == 0)
            {
                return 0;
            }

            if (value.Length > source.Length)
            {
                // A non-linguistic search compares chars directly against one another, so large
                // target strings can never be found inside small search spaces. This check also
                // handles empty 'source' spans.

                return -1;
            }

            return OrdinalCasing.IndexOf(source, value);
        }

        internal static int LastIndexOf(string source, string value, int startIndex, int count)
        {
            int result = source.AsSpan(startIndex, count).LastIndexOf(value);
            if (result >= 0) { result += startIndex; } // if match found, adjust 'result' by the actual start position
            return result;
        }

        internal static unsafe int LastIndexOf(string source, string value, int startIndex, int count, bool ignoreCase)
        {
            if (source == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            if (value == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }

            if (value.Length == 0)
            {
                return startIndex + 1; // startIndex is the index of the last char to include in the search space
            }

            if (count == 0)
            {
                return -1;
            }

            if (!ignoreCase)
            {
                LastIndexOf(source, value, startIndex, count);
            }

            if (!source.TryGetSpan(startIndex, count, out ReadOnlySpan<char> sourceSpan))
            {
                // Bounds check failed - figure out exactly what went wrong so that we can
                // surface the correct argument exception.

                if ((uint)startIndex > (uint)source.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
                }
                else
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
                }
            }

            int result = OrdinalCasing.LastIndexOf(sourceSpan, value);

            if (result >= 0)
            {
                result += startIndex;
            }
            return result;
        }

        internal static int LastIndexOfOrdinalIgnoreCase(ReadOnlySpan<char> source, ReadOnlySpan<char> value)
        {
            if (value.Length == 0)
            {
                return source.Length;
            }

            if (value.Length > source.Length)
            {
                // A non-linguistic search compares chars directly against one another, so large
                // target strings can never be found inside small search spaces. This check also
                // handles empty 'source' spans.

                return -1;
            }

            return OrdinalCasing.LastIndexOf(source, value);
        }

        internal static int ToUpperOrdinal(ReadOnlySpan<char> source, Span<char> destination)
        {
            if (source.Overlaps(destination))
                throw new InvalidOperationException("This operation is invalid on overlapping buffers.");

            // Assuming that changing case does not affect length
            if (destination.Length < source.Length)
                return -1;

            OrdinalCasing.ToUpperOrdinal(source, destination);
            return source.Length;
        }
    }
}

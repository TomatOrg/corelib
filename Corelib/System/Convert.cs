namespace System;

public static class Convert
{
    
        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with uppercase hex characters.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <returns>The string representation in hex of the elements in <paramref name="inArray"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <code>null</code>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="inArray"/> is too large to be encoded.</exception>
        public static string ToHexString(byte[] inArray)
        {
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));

            return ToHexString(new ReadOnlySpan<byte>(inArray));
        }

        /// <summary>
        /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with uppercase hex characters.
        /// Parameters specify the subset as an offset in the input array and the number of elements in the array to convert.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="offset">An offset in <paramref name="inArray"/>.</param>
        /// <param name="length">The number of elements of <paramref name="inArray"/> to convert.</param>
        /// <returns>The string representation in hex of <paramref name="length"/> elements of <paramref name="inArray"/>, starting at position <paramref name="offset"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <code>null</code>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="length"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> plus <paramref name="length"/> is greater than the length of <paramref name="inArray"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="inArray"/> is too large to be encoded.</exception>
        public static string ToHexString(byte[] inArray, int offset, int length)
        {
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), ArgumentOutOfRangeException.Index);
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), ArgumentOutOfRangeException.GenericPositive);
            if (offset > (inArray.Length - length))
                throw new ArgumentOutOfRangeException(nameof(offset), ArgumentOutOfRangeException.OffsetLength);

            return ToHexString(new ReadOnlySpan<byte>(inArray, offset, length));
        }

        /// <summary>
        /// Converts a span of 8-bit unsigned integers to its equivalent string representation that is encoded with uppercase hex characters.
        /// </summary>
        /// <param name="bytes">A span of 8-bit unsigned integers.</param>
        /// <returns>The string representation in hex of the elements in <paramref name="bytes"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bytes"/> is too large to be encoded.</exception>
        public static string ToHexString(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length == 0)
                return string.Empty;
            if (bytes.Length > int.MaxValue / 2)
                throw new ArgumentOutOfRangeException(nameof(bytes), ArgumentOutOfRangeException.InputTooLarge);

            return HexConverter.ToString(bytes, HexConverter.Casing.Upper);
        }
}
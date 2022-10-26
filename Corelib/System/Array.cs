using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System;

[StructLayout(LayoutKind.Sequential)]
public class Array
{
    
    // This is the threshold where Introspective sort switches to Insertion sort.
    // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
    // Large value types may benefit from a smaller number.
    internal const int IntrosortSizeThreshold = 16;

    public static int MaxLength => int.MaxValue;

    #region Instance
    
    private readonly int _length;

    public int Length => _length;
    public long LongLength => _length;
    public int Rank => 1;

    private Array() {}

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    internal extern unsafe void* GetDataPtr();

    #endregion
    
    private static class EmptyArray<T>
    {
        internal static readonly T[] Value = new T[0];
    }

    public static T[] Empty<T>()
    {
        return EmptyArray<T>.Value;
    }
    
    public object Clone()
    {
        return MemberwiseClone();
    }

    #region Clear

    public static void Clear<T>(T[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        
        array.AsSpan().Clear();
    }

    public static void Clear<T>(T[] array, int index, int length)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        
        array.AsSpan(index, length).Clear();
    }

    #endregion

    #region Copy

    public static void Copy<T>(T[] sourceArray, T[] destinationArray, int length)
    {
        sourceArray.AsSpan(0, length).CopyTo(destinationArray);
    }
    
    public static void Copy<T>(T[] sourceArray, int sourceIndex, T[] destinationArray, int destinationIndex, int length)
    {
        sourceArray.AsSpan(sourceIndex, length).CopyTo(destinationArray.AsSpan(destinationIndex));
    }

    #endregion

    #region Fill

    public static void Fill<T>(T[] array, T value)
    {
        array.AsSpan().Fill(value);
        Fill(array, value, 0, array.Length);
    }
    
    public static void Fill<T>(T[] array, T value, int startIndex, int count)
    {
        array.AsSpan(startIndex, count).Fill(value);
    }

    #endregion
    
    #region IndexOf
    
    public static int IndexOf<T>(T[] array, T value)
    {
        if (array == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        return IndexOf(array, value, 0, array.Length);
    }

    public static int IndexOf<T>(T[] array, T value, int startIndex)
    {
        if (array == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        return IndexOf(array, value, startIndex, array.Length - startIndex);
    }

    public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
    {
        if (array == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        if ((uint)startIndex > (uint)array.Length)
        {
            ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
        }

        if ((uint)count > (uint)(array.Length - startIndex))
        {
            ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
        }

        if (RuntimeHelpers.IsBitwiseEquatable<T>())
        {
            if (Unsafe.SizeOf<T>() == sizeof(byte))
            {
                int result = SpanHelpers.IndexOf(
                    ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Unsafe.As<byte[]>(array)), startIndex),
                    Unsafe.As<T, byte>(ref value),
                    count);
                return (result >= 0 ? startIndex : 0) + result;
            }
            else if (Unsafe.SizeOf<T>() == sizeof(char))
            {
                int result = SpanHelpers.IndexOf(
                    ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Unsafe.As<char[]>(array)), startIndex),
                    Unsafe.As<T, char>(ref value),
                    count);
                return (result >= 0 ? startIndex : 0) + result;
            }
            else if (Unsafe.SizeOf<T>() == sizeof(int))
            {
                int result = SpanHelpers.IndexOf(
                    ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Unsafe.As<int[]>(array)), startIndex),
                    Unsafe.As<T, int>(ref value),
                    count);
                return (result >= 0 ? startIndex : 0) + result;
            }
            else if (Unsafe.SizeOf<T>() == sizeof(long))
            {
                int result = SpanHelpers.IndexOf(
                    ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Unsafe.As<long[]>(array)), startIndex),
                    Unsafe.As<T, long>(ref value),
                    count);
                return (result >= 0 ? startIndex : 0) + result;
            }
        }

        return EqualityComparer<T>.Default.IndexOf(array, value, startIndex, count);
    }
        
    #endregion

    #region LastIndexOf
    
    public static int LastIndexOf<T>(T[] array, T value)
    {
        if (array == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        return LastIndexOf(array, value, array.Length - 1, array.Length);
    }

    public static int LastIndexOf<T>(T[] array, T value, int startIndex)
    {
        if (array == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }
        // if array is empty and startIndex is 0, we need to pass 0 as count
        return LastIndexOf(array, value, startIndex, (array.Length == 0) ? 0 : (startIndex + 1));
    }

    public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count)
    {
        if (array == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        if (array.Length == 0)
        {
            //
            // Special case for 0 length List
            // accept -1 and 0 as valid startIndex for compablility reason.
            //
            if (startIndex != -1 && startIndex != 0)
            {
                ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
            }

            // only 0 is a valid value for count if array is empty
            if (count != 0)
            {
                ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
            }
            return -1;
        }

        // Make sure we're not out of range
        if ((uint)startIndex >= (uint)array.Length)
        {
            ThrowHelper.ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index();
        }

        // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
        if (count < 0 || startIndex - count + 1 < 0)
        {
            ThrowHelper.ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count();
        }

        if (RuntimeHelpers.IsBitwiseEquatable<T>())
        {
            if (Unsafe.SizeOf<T>() == sizeof(byte))
            {
                int endIndex = startIndex - count + 1;
                int result = SpanHelpers.LastIndexOf(
                    ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Unsafe.As<byte[]>(array)), endIndex),
                    Unsafe.As<T, byte>(ref value),
                    count);

                return (result >= 0 ? endIndex : 0) + result;
            }
            else if (Unsafe.SizeOf<T>() == sizeof(char))
            {
                int endIndex = startIndex - count + 1;
                int result = SpanHelpers.LastIndexOf(
                    ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Unsafe.As<char[]>(array)), endIndex),
                    Unsafe.As<T, char>(ref value),
                    count);

                return (result >= 0 ? endIndex : 0) + result;
            }
            else if (Unsafe.SizeOf<T>() == sizeof(int))
            {
                int endIndex = startIndex - count + 1;
                int result = SpanHelpers.LastIndexOf(
                    ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Unsafe.As<int[]>(array)), endIndex),
                    Unsafe.As<T, int>(ref value),
                    count);

                return (result >= 0 ? endIndex : 0) + result;
            }
            else if (Unsafe.SizeOf<T>() == sizeof(long))
            {
                int endIndex = startIndex - count + 1;
                int result = SpanHelpers.LastIndexOf(
                    ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Unsafe.As<long[]>(array)), endIndex),
                    Unsafe.As<T, long>(ref value),
                    count);

                return (result >= 0 ? endIndex : 0) + result;
            }
        }

        return EqualityComparer<T>.Default.LastIndexOf(array, value, startIndex, count);
    }

    #endregion

    #region Reverse

    public static void Reverse<T>(T[] array)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        Reverse(array, 0, array.Length);
    }

    public static void Reverse<T>(T[] array, int index, int length)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        if (index < 0)
            ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        if (length < 0)
            ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
        if (array.Length - index < length)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

        if (length <= 1)
            return;

        ref T first = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
        ref T last = ref Unsafe.Add(ref Unsafe.Add(ref first, length), -1);
        do
        {
            T temp = first;
            first = last;
            last = temp;
            first = ref Unsafe.Add(ref first, 1);
            last = ref Unsafe.Add(ref last, -1);
        } while (Unsafe.IsAddressLessThan(ref first, ref last));
    }

    #endregion

    #region Sort

    public static void Sort<T>(T[] array)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);

        if (array.Length > 1)
        {
            var span = new Span<T>(ref MemoryMarshal.GetArrayDataReference(array), array.Length);
            ArraySortHelper<T>.Default.Sort(span, null);
        }
    }

    public static void Sort<TKey, TValue>(TKey[] keys, TValue[]? items)
    {
        if (keys == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
        Sort<TKey, TValue>(keys, items, 0, keys.Length, null);
    }

    public static void Sort<T>(T[] array, int index, int length)
    {
        Sort<T>(array, index, length, null);
    }

    public static void Sort<TKey, TValue>(TKey[] keys, TValue[]? items, int index, int length)
    {
        Sort<TKey, TValue>(keys, items, index, length, null);
    }

    public static void Sort<T>(T[] array, System.Collections.Generic.IComparer<T>? comparer)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        Sort<T>(array, 0, array.Length, comparer);
    }

    public static void Sort<TKey, TValue>(TKey[] keys, TValue[]? items, System.Collections.Generic.IComparer<TKey>? comparer)
    {
        if (keys == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
        Sort<TKey, TValue>(keys, items, 0, keys.Length, comparer);
    }

    public static void Sort<T>(T[] array, int index, int length, System.Collections.Generic.IComparer<T>? comparer)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        if (index < 0)
            ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        if (length < 0)
            ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
        if (array.Length - index < length)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

        if (length > 1)
        {
            var span = new Span<T>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index), length);
            ArraySortHelper<T>.Default.Sort(span, comparer);
        }
    }

    public static void Sort<TKey, TValue>(TKey[] keys, TValue[]? items, int index, int length, System.Collections.Generic.IComparer<TKey>? comparer)
    {
        if (keys == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.keys);
        if (index < 0)
            ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        if (length < 0)
            ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();
        if (keys.Length - index < length || (items != null && index > items.Length - length))
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

        if (length > 1)
        {
            if (items == null)
            {
                Sort<TKey>(keys, index, length, comparer);
                return;
            }

            var spanKeys = new Span<TKey>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(keys), index), length);
            var spanItems = new Span<TValue>(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(items), index), length);
            ArraySortHelper<TKey, TValue>.Default.Sort(spanKeys, spanItems, comparer);
        }
    }

    public static void Sort<T>(T[] array, Comparison<T> comparison)
    {
        if (array == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        }

        if (comparison == null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparison);
        }

        var span = new Span<T>(ref MemoryMarshal.GetArrayDataReference(array), array.Length);
        ArraySortHelper<T>.Sort(span, comparison);
    }

    #endregion

    #region BinarySearch

    public static int BinarySearch<T>(T[] array, T value)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        return BinarySearch<T>(array, 0, array.Length, value, null);
    }

    public static int BinarySearch<T>(T[] array, T value, System.Collections.Generic.IComparer<T>? comparer)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        return BinarySearch<T>(array, 0, array.Length, value, comparer);
    }

    public static int BinarySearch<T>(T[] array, int index, int length, T value)
    {
        return BinarySearch<T>(array, index, length, value, null);
    }

    public static int BinarySearch<T>(T[] array, int index, int length, T value, System.Collections.Generic.IComparer<T>? comparer)
    {
        if (array == null)
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
        if (index < 0)
            ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
        if (length < 0)
            ThrowHelper.ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum();

        if (array.Length - index < length)
            ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);

        return ArraySortHelper<T>.Default.BinarySearch(array, index, length, value, comparer);
    }

    #endregion
    
    #region Resize

    public static void Resize<T>(ref T[] array, int newSize)
    {
        if (array == null)
        {
            array = new T[newSize];
        }
        else if (array.Length < newSize)
        {
            var arr = new T[newSize];
            Copy(array, arr, array.Length);
            array = arr;
        }
    }

    #endregion

    private class GenericArray<T> : Array, ICollection<T>, IList<T>, IEnumerable<T>
    {
        
        private class GenericEnumerator : IEnumerator<T>
        {

            private T[] _array;
            private int _index;

            public GenericEnumerator(T[] array)
            {
                _array = array;
                _index = -1;
            }

            public T Current
            {
                get
                {
                    if (_index < 0)
                        throw new InvalidOperationException("Enumeration has not started");
                    if (_index >= _array._length)
                        throw new InvalidOperationException("Enumeration has finished");
                    return _array[_index];
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _index++;
                return (_index < _array._length);
            }

            public void Reset()
            {
                _index = -1;
            }
        
            public void Dispose()
            {
            }

        }

        public IEnumerator<T> GetEnumerator()
        {
            return new GenericEnumerator(Unsafe.As<T[]>(this));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _length;
        public bool IsReadOnly => false;
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            Array.Clear(Unsafe.As<T[]>(this));
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(Unsafe.As<T[]>(this), item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Unsafe.As<T[]>(this).AsSpan(0, arrayIndex).CopyTo(array);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public T this[int index]
        {
            get => Unsafe.As<T[]>(this)[index];
            set => Unsafe.As<T[]>(this)[index] = value;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(Unsafe.As<T[]>(this), item);
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
    }
    

}
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


// This file defines an internal static class used to throw exceptions in BCL code.
// The main purpose is to reduce code size.
//
// The old way to throw an exception generates quite a lot IL code and assembly code.
// Following is an example:
//     C# source
//          throw new ArgumentNullException(nameof(key), SR.ArgumentNull_Key);
//     IL code:
//          IL_0003:  ldstr      "key"
//          IL_0008:  ldstr      "ArgumentNull_Key"
//          IL_000d:  call       string System.Environment::GetResourceString(string)
//          IL_0012:  newobj     instance void System.ArgumentNullException::.ctor(string,string)
//          IL_0017:  throw
//    which is 21bytes in IL.
//
// So we want to get rid of the ldstr and call to Environment.GetResource in IL.
// In order to do that, I created two enums: ExceptionResource, ExceptionArgument to represent the
// argument name and resource name in a small integer. The source code will be changed to
//    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key, ExceptionResource.ArgumentNull_Key);
//
// The IL code will be 7 bytes.
//    IL_0008:  ldc.i4.4
//    IL_0009:  ldc.i4.4
//    IL_000a:  call       void System.ThrowHelper::ThrowArgumentNullException(valuetype System.ExceptionArgument)
//    IL_000f:  ldarg.0
//
// This will also reduce the Jitted code size a lot.
//
// It is very important we do this for generic classes because we can easily generate the same code
// multiple times for different instantiation.
//

using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System
{
    [StackTraceHidden]
    internal static class ThrowHelper
    {
        
        [DoesNotReturn]
        internal static void ThrowKeyNullException() => ThrowArgumentNullException("key");

        [DoesNotReturn]
        internal static void ThrowArgumentNullException(string name) => throw new ArgumentNullException(name);

        [DoesNotReturn]
        internal static void ThrowArgumentNullException(string name, string message) => throw new ArgumentNullException(name, message);

        [DoesNotReturn]
        internal static void ThrowValueNullException() => throw new ArgumentException("The value was of an incorrect type for this dictionary.");

        // [DoesNotReturn]
        // internal static void ThrowArrayTypeMismatchException()
        // {
        //     throw new ArrayTypeMismatchException();
        // }

        // [DoesNotReturn]
        // internal static void ThrowInvalidTypeWithPointersNotSupported(Type targetType)
        // {
        //     throw new ArgumentException(SR.Format(SR.Argument_InvalidTypeWithPointersNotSupported, targetType));
        // }

        [DoesNotReturn]
        internal static void ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException();
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException_DestinationTooShort()
        {
            throw new ArgumentException("Destination is too short.", "destination");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException_OverlapAlignmentMismatch()
        {
            throw new ArgumentException("Overlapping spans have mismatching alignment.");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException_CannotExtractScalar(ExceptionArgument argument)
        {
            throw GetArgumentException(ExceptionResource.Argument_CannotExtractScalar, argument);
        }

        // [DoesNotReturn]
        // internal static void ThrowArgumentException_TupleIncorrectType(object obj)
        // {
        //     throw new ArgumentException(SR.Format(SR.ArgumentException_ValueTupleIncorrectType, obj.GetType()), "other");
        // }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRange_IndexException()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.index,
                                                    ExceptionResource.ArgumentOutOfRange_Index);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException_BadComparer(object? comparer)
        {
            throw new ArgumentException($"Unable to sort because the IComparer.Compare() method returns inconsistent results. Either a value does not compare equal to itself, or one value repeatedly compared to another value yields different results. IComparer: '{comparer}'.");
        }

        [DoesNotReturn]
        internal static void ThrowIndexArgumentOutOfRange_NeedNonNegNumException()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.index,
                                                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        [DoesNotReturn]
        internal static void ThrowValueArgumentOutOfRange_NeedNonNegNumException()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.value,
                                                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        [DoesNotReturn]
        internal static void ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.length,
                                                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        [DoesNotReturn]
        internal static void ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.startIndex,
                                                    ExceptionResource.ArgumentOutOfRange_Index);
        }

        [DoesNotReturn]
        internal static void ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.count,
                                                    ExceptionResource.ArgumentOutOfRange_Count);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRange_Year()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.year,
                                                    ExceptionResource.ArgumentOutOfRange_Year);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRange_Month(int month)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between one and twelve.");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRange_DayNumber(int dayNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(dayNumber), dayNumber, "Day number must be between 0 and DateOnly.MaxValue.DayNumber.");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRange_BadYearMonthDay()
        {
            throw new ArgumentOutOfRangeException(null, "Year, Month, and Day parameters describe an un-representable DateTime.");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRange_BadHourMinuteSecond()
        {
            throw new ArgumentOutOfRangeException(null, "Hour, Minute, and Second parameters describe an un-representable DateTime.");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRange_TimeSpanTooLong()
        {
            throw new ArgumentOutOfRangeException(null, "TimeSpan overflowed because the duration is too long.");
        }

        // [DoesNotReturn]
        // internal static void ThrowWrongKeyTypeArgumentException<T>(T key, Type targetType)
        // {
        //     // Generic key to move the boxing to the right hand side of throw
        //     throw GetWrongKeyTypeArgumentException((object?)key, targetType);
        // }
        //
        // [DoesNotReturn]
        // internal static void ThrowWrongValueTypeArgumentException<T>(T value, Type targetType)
        // {
        //     // Generic key to move the boxing to the right hand side of throw
        //     throw GetWrongValueTypeArgumentException((object?)value, targetType);
        // }

        private static ArgumentException GetAddingDuplicateWithKeyArgumentException(object? key)
        {
            return new ArgumentException($"An item with the same key has already been added. Key: {key}");
        }

        [DoesNotReturn]
        internal static void ThrowAddingDuplicateWithKeyArgumentException<T>(T key)
        {
            // Generic key to move the boxing to the right hand side of throw
            throw GetAddingDuplicateWithKeyArgumentException((object?)key);
        }

        [DoesNotReturn]
        internal static void ThrowKeyNotFoundException<T>(T key)
        {
            // Generic key to move the boxing to the right hand side of throw
            throw GetKeyNotFoundException((object?)key);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException(ExceptionResource resource)
        {
            throw GetArgumentException(resource);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentException(ExceptionResource resource, ExceptionArgument argument)
        {
            throw GetArgumentException(resource, argument);
        }

        // [DoesNotReturn]
        // internal static void ThrowArgumentException_HandleNotSync(string paramName)
        // {
        //     throw new ArgumentException(SR.Arg_HandleNotSync, paramName);
        // }
        //
        // [DoesNotReturn]
        // internal static void ThrowArgumentException_HandleNotAsync(string paramName)
        // {
        //     throw new ArgumentException(SR.Arg_HandleNotAsync, paramName);
        // }

        [DoesNotReturn]
        internal static void ThrowArgumentNullException(ExceptionArgument argument)
        {
            throw new ArgumentNullException(GetArgumentName(argument));
        }

        [DoesNotReturn]
        internal static void ThrowArgumentNullException(ExceptionResource resource)
        {
            throw new ArgumentNullException(GetResourceString(resource));
        }

        [DoesNotReturn]
        internal static void ThrowArgumentNullException(ExceptionArgument argument, ExceptionResource resource)
        {
            throw new ArgumentNullException(GetArgumentName(argument), GetResourceString(resource));
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
        {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument));
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
        {
            throw GetArgumentOutOfRangeException(argument, resource);
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, int paramNumber, ExceptionResource resource)
        {
            throw GetArgumentOutOfRangeException(argument, paramNumber, resource);
        }

        [DoesNotReturn]
        internal static void ThrowEndOfFileException()
        {
            throw CreateEndOfFileException();
        }

        internal static Exception CreateEndOfFileException() =>
            new EndOfStreamException("Unable to read beyond the end of the stream.");

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException(ExceptionResource resource)
        {
            throw GetInvalidOperationException(resource);
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_OutstandingReferences()
        {
            throw new InvalidOperationException("Release all references before disposing this instance.");
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException(ExceptionResource resource, Exception e)
        {
            throw new InvalidOperationException(GetResourceString(resource), e);
        }

        // [DoesNotReturn]
        // internal static void ThrowSerializationException(ExceptionResource resource)
        // {
        //     throw new SerializationException(GetResourceString(resource));
        // }

        // [DoesNotReturn]
        // internal static void ThrowSecurityException(ExceptionResource resource)
        // {
        //     throw new System.Security.SecurityException(GetResourceString(resource));
        // }

        // [DoesNotReturn]
        // internal static void ThrowRankException(ExceptionResource resource)
        // {
        //     throw new RankException(GetResourceString(resource));
        // }

        [DoesNotReturn]
        internal static void ThrowNotSupportedException(ExceptionResource resource)
        {
            throw new NotSupportedException(GetResourceString(resource));
        }

        [DoesNotReturn]
        internal static void ThrowNotSupportedException_UnseekableStream()
        {
            throw new NotSupportedException("Stream does not support seeking.");
        }

        [DoesNotReturn]
        internal static void ThrowNotSupportedException_UnreadableStream()
        {
            throw new NotSupportedException("Stream does not support reading.");
        }

        [DoesNotReturn]
        internal static void ThrowNotSupportedException_UnwritableStream()
        {
            throw new NotSupportedException("Stream does not support writing.");
        }

        // [DoesNotReturn]
        // internal static void ThrowUnauthorizedAccessException(ExceptionResource resource)
        // {
        //     throw new UnauthorizedAccessException(GetResourceString(resource));
        // }

        [DoesNotReturn]
        internal static void ThrowObjectDisposedException(string objectName, ExceptionResource resource)
        {
            throw new ObjectDisposedException(objectName, GetResourceString(resource));
        }

        [DoesNotReturn]
        internal static void ThrowObjectDisposedException_StreamClosed(string? objectName)
        {
            throw new ObjectDisposedException(objectName, "Cannot access a closed Stream.");
        }

        [DoesNotReturn]
        internal static void ThrowObjectDisposedException_FileClosed()
        {
            throw new ObjectDisposedException(null, "Cannot access a closed file.");
        }

        [DoesNotReturn]
        internal static void ThrowObjectDisposedException(ExceptionResource resource)
        {
            throw new ObjectDisposedException(null, GetResourceString(resource));
        }

        [DoesNotReturn]
        internal static void ThrowNotSupportedException()
        {
            throw new NotSupportedException();
        }

        [DoesNotReturn]
        internal static void ThrowAggregateException(List<Exception> exceptions)
        {
            throw new AggregateException(exceptions);
        }

        [DoesNotReturn]
        internal static void ThrowOutOfMemoryException()
        {
            throw new OutOfMemoryException();
        }

        // [DoesNotReturn]
        // internal static void ThrowArgumentException_Argument_InvalidArrayType()
        // {
        //     throw new ArgumentException(SR.Argument_InvalidArrayType);
        // }

        // [DoesNotReturn]
        // internal static void ThrowArgumentException_InvalidHandle(string? paramName)
        // {
        //     throw new ArgumentException(SR.Arg_InvalidHandle, paramName);
        // }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_InvalidOperation_EnumNotStarted()
        {
            throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_InvalidOperation_EnumEnded()
        {
            throw new InvalidOperationException("Enumeration already finished.");
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_EnumCurrent(int index)
        {
            throw GetInvalidOperationException_EnumCurrent(index);
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion()
        {
            throw new InvalidOperationException("Collection was modified after the enumerator was instantiated.");
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen()
        {
            throw new InvalidOperationException("Enumeration has either not started or has already finished.");
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_InvalidOperation_NoValue()
        {
            throw new InvalidOperationException("Nullable object must have a value.");
        }

        [DoesNotReturn]
        internal static void ThrowInvalidOperationException_ConcurrentOperationsNotSupported()
        {
            throw new InvalidOperationException("Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.");
        }

        // [DoesNotReturn]
        // internal static void ThrowInvalidOperationException_HandleIsNotInitialized()
        // {
        //     throw new InvalidOperationException(SR.InvalidOperation_HandleIsNotInitialized);
        // }
        //
        // [DoesNotReturn]
        // internal static void ThrowInvalidOperationException_HandleIsNotPinned()
        // {
        //     throw new InvalidOperationException(SR.InvalidOperation_HandleIsNotPinned);
        // }

        [DoesNotReturn]
        internal static void ThrowArraySegmentCtorValidationFailedExceptions(Array? array, int offset, int count)
        {
            throw GetArraySegmentCtorValidationFailedException(array, offset, count);
        }

        [DoesNotReturn]
        internal static void ThrowFormatException_BadFormatSpecifier()
        {
            throw new FormatException("Format specifier was invalid.");
        }

        // [DoesNotReturn]
        // internal static void ThrowArgumentOutOfRangeException_PrecisionTooLarge()
        // {
        //     throw new ArgumentOutOfRangeException("precision", SR.Format(SR.Argument_PrecisionTooLarge, StandardFormat.MaxPrecision));
        // }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException_SymbolDoesNotFit()
        {
            throw new ArgumentOutOfRangeException("symbol", "Format specifier was invalid.");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException_NeedPosNum(string? paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, "Positive number required.");
        }

        [DoesNotReturn]
        internal static void ThrowArgumentOutOfRangeException_NeedNonNegNum(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName, "Non-negative number required.");
        }

        [DoesNotReturn]
        internal static void ArgumentOutOfRangeException_Enum_Value()
        {
            throw new ArgumentOutOfRangeException("value", "Enum value was out of legal range.");
        }

        private static Exception GetArraySegmentCtorValidationFailedException(Array? array, int offset, int count)
        {
            if (array == null)
                return new ArgumentNullException(nameof(array));
            if (offset < 0)
                return new ArgumentOutOfRangeException(nameof(offset), "Non-negative number required.");
            if (count < 0)
                return new ArgumentOutOfRangeException(nameof(count), "Non-negative number required.");

            Debug.Assert(array.Length - offset < count);
            return new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
        }

        private static ArgumentException GetArgumentException(ExceptionResource resource)
        {
            return new ArgumentException(GetResourceString(resource));
        }

        private static InvalidOperationException GetInvalidOperationException(ExceptionResource resource)
        {
            return new InvalidOperationException(GetResourceString(resource));
        }

        // private static ArgumentException GetWrongKeyTypeArgumentException(object? key, Type targetType)
        // {
        //     return new ArgumentException(SR.Format(SR.Arg_WrongType, key, targetType), nameof(key));
        // }
        //
        // private static ArgumentException GetWrongValueTypeArgumentException(object? value, Type targetType)
        // {
        //     return new ArgumentException(SR.Format(SR.Arg_WrongType, value, targetType), nameof(value));
        // }

        private static KeyNotFoundException GetKeyNotFoundException(object? key)
        {
            return new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
        }

        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument), GetResourceString(resource));
        }

        private static ArgumentException GetArgumentException(ExceptionResource resource, ExceptionArgument argument)
        {
            return new ArgumentException(GetResourceString(resource), GetArgumentName(argument));
        }

        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument, int paramNumber, ExceptionResource resource)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument) + "[" + paramNumber.ToString() + "]", GetResourceString(resource));
        }

        private static InvalidOperationException GetInvalidOperationException_EnumCurrent(int index)
        {
            return new InvalidOperationException(
                index < 0 ?
                "Enumeration has not started. Call MoveNext." :
                "Enumeration already finished.");
        }

        // Allow nulls for reference types and Nullable<U>, but not for value types.
        // Aggressively inline so the jit evaluates the if in place and either drops the call altogether
        // Or just leaves null test and call to the Non-returning ThrowHelper.ThrowArgumentNullException
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IfNullAndNullsAreIllegalThenThrow<T>(object? value, ExceptionArgument argName)
        {
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>.
            if (!(default(T) == null) && value == null)
                ThrowHelper.ThrowArgumentNullException(argName);
        }

        // Throws if 'T' is disallowed in Vector<T> in the Numerics namespace.
        // If 'T' is allowed, no-ops. JIT will elide the method entirely if 'T'
        // is supported and we're on an optimized release build.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowForUnsupportedNumericsVectorBaseType<T>() where T : struct
        {
            if (typeof(T) != typeof(byte) && typeof(T) != typeof(sbyte) &&
                typeof(T) != typeof(short) && typeof(T) != typeof(ushort) &&
                typeof(T) != typeof(int) && typeof(T) != typeof(uint) &&
                typeof(T) != typeof(long) && typeof(T) != typeof(ulong) &&
                typeof(T) != typeof(float) && typeof(T) != typeof(double) &&
                typeof(T) != typeof(nint) && typeof(T) != typeof(nuint))
            {
                ThrowNotSupportedException(ExceptionResource.Arg_TypeNotSupported);
            }
        }

        // Throws if 'T' is disallowed in Vector64/128/256<T> in the Intrinsics namespace.
        // If 'T' is allowed, no-ops. JIT will elide the method entirely if 'T'
        // is supported and we're on an optimized release build.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowForUnsupportedIntrinsicsVectorBaseType<T>() where T : struct
        {
            if (typeof(T) != typeof(byte) && typeof(T) != typeof(sbyte) &&
                typeof(T) != typeof(short) && typeof(T) != typeof(ushort) &&
                typeof(T) != typeof(int) && typeof(T) != typeof(uint) &&
                typeof(T) != typeof(long) && typeof(T) != typeof(ulong) &&
                typeof(T) != typeof(float) && typeof(T) != typeof(double))
            {
                ThrowNotSupportedException(ExceptionResource.Arg_TypeNotSupported);
            }
        }

#if false // Reflection-based implementation does not work for CoreRT/ProjectN
        // This function will convert an ExceptionArgument enum value to the argument name string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetArgumentName(ExceptionArgument argument)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionArgument), argument),
                "The enum value is not defined, please check the ExceptionArgument Enum.");

            return argument.ToString();
        }
#endif

        private static string GetArgumentName(ExceptionArgument argument)
        {
            switch (argument)
            {
                case ExceptionArgument.obj:
                    return "obj";
                case ExceptionArgument.dictionary:
                    return "dictionary";
                case ExceptionArgument.array:
                    return "array";
                case ExceptionArgument.info:
                    return "info";
                case ExceptionArgument.key:
                    return "key";
                case ExceptionArgument.text:
                    return "text";
                case ExceptionArgument.values:
                    return "values";
                case ExceptionArgument.value:
                    return "value";
                case ExceptionArgument.startIndex:
                    return "startIndex";
                case ExceptionArgument.task:
                    return "task";
                case ExceptionArgument.bytes:
                    return "bytes";
                case ExceptionArgument.byteIndex:
                    return "byteIndex";
                case ExceptionArgument.byteCount:
                    return "byteCount";
                case ExceptionArgument.ch:
                    return "ch";
                case ExceptionArgument.chars:
                    return "chars";
                case ExceptionArgument.charIndex:
                    return "charIndex";
                case ExceptionArgument.charCount:
                    return "charCount";
                case ExceptionArgument.s:
                    return "s";
                case ExceptionArgument.input:
                    return "input";
                case ExceptionArgument.ownedMemory:
                    return "ownedMemory";
                case ExceptionArgument.list:
                    return "list";
                case ExceptionArgument.index:
                    return "index";
                case ExceptionArgument.capacity:
                    return "capacity";
                case ExceptionArgument.collection:
                    return "collection";
                case ExceptionArgument.item:
                    return "item";
                case ExceptionArgument.converter:
                    return "converter";
                case ExceptionArgument.match:
                    return "match";
                case ExceptionArgument.count:
                    return "count";
                case ExceptionArgument.action:
                    return "action";
                case ExceptionArgument.comparison:
                    return "comparison";
                case ExceptionArgument.exceptions:
                    return "exceptions";
                case ExceptionArgument.exception:
                    return "exception";
                case ExceptionArgument.pointer:
                    return "pointer";
                case ExceptionArgument.start:
                    return "start";
                case ExceptionArgument.format:
                    return "format";
                case ExceptionArgument.formats:
                    return "formats";
                case ExceptionArgument.culture:
                    return "culture";
                case ExceptionArgument.comparer:
                    return "comparer";
                case ExceptionArgument.comparable:
                    return "comparable";
                case ExceptionArgument.source:
                    return "source";
                case ExceptionArgument.state:
                    return "state";
                case ExceptionArgument.length:
                    return "length";
                case ExceptionArgument.comparisonType:
                    return "comparisonType";
                case ExceptionArgument.manager:
                    return "manager";
                case ExceptionArgument.sourceBytesToCopy:
                    return "sourceBytesToCopy";
                case ExceptionArgument.callBack:
                    return "callBack";
                case ExceptionArgument.creationOptions:
                    return "creationOptions";
                case ExceptionArgument.function:
                    return "function";
                case ExceptionArgument.scheduler:
                    return "scheduler";
                case ExceptionArgument.continuationAction:
                    return "continuationAction";
                case ExceptionArgument.continuationFunction:
                    return "continuationFunction";
                case ExceptionArgument.tasks:
                    return "tasks";
                case ExceptionArgument.asyncResult:
                    return "asyncResult";
                case ExceptionArgument.beginMethod:
                    return "beginMethod";
                case ExceptionArgument.endMethod:
                    return "endMethod";
                case ExceptionArgument.endFunction:
                    return "endFunction";
                case ExceptionArgument.cancellationToken:
                    return "cancellationToken";
                case ExceptionArgument.continuationOptions:
                    return "continuationOptions";
                case ExceptionArgument.delay:
                    return "delay";
                case ExceptionArgument.millisecondsDelay:
                    return "millisecondsDelay";
                case ExceptionArgument.millisecondsTimeout:
                    return "millisecondsTimeout";
                case ExceptionArgument.stateMachine:
                    return "stateMachine";
                case ExceptionArgument.timeout:
                    return "timeout";
                case ExceptionArgument.type:
                    return "type";
                case ExceptionArgument.sourceIndex:
                    return "sourceIndex";
                case ExceptionArgument.sourceArray:
                    return "sourceArray";
                case ExceptionArgument.destinationIndex:
                    return "destinationIndex";
                case ExceptionArgument.destinationArray:
                    return "destinationArray";
                case ExceptionArgument.pHandle:
                    return "pHandle";
                case ExceptionArgument.handle:
                    return "handle";
                case ExceptionArgument.other:
                    return "other";
                case ExceptionArgument.newSize:
                    return "newSize";
                case ExceptionArgument.lowerBounds:
                    return "lowerBounds";
                case ExceptionArgument.lengths:
                    return "lengths";
                case ExceptionArgument.len:
                    return "len";
                case ExceptionArgument.keys:
                    return "keys";
                case ExceptionArgument.indices:
                    return "indices";
                case ExceptionArgument.index1:
                    return "index1";
                case ExceptionArgument.index2:
                    return "index2";
                case ExceptionArgument.index3:
                    return "index3";
                case ExceptionArgument.length1:
                    return "length1";
                case ExceptionArgument.length2:
                    return "length2";
                case ExceptionArgument.length3:
                    return "length3";
                case ExceptionArgument.endIndex:
                    return "endIndex";
                case ExceptionArgument.elementType:
                    return "elementType";
                case ExceptionArgument.arrayIndex:
                    return "arrayIndex";
                case ExceptionArgument.year:
                    return "year";
                case ExceptionArgument.codePoint:
                    return "codePoint";
                case ExceptionArgument.str:
                    return "str";
                case ExceptionArgument.options:
                    return "options";
                case ExceptionArgument.prefix:
                    return "prefix";
                case ExceptionArgument.suffix:
                    return "suffix";
                case ExceptionArgument.buffer:
                    return "buffer";
                case ExceptionArgument.buffers:
                    return "buffers";
                case ExceptionArgument.offset:
                    return "offset";
                case ExceptionArgument.stream:
                    return "stream";
                default:
                    Debug.Fail("The enum value is not defined, please check the ExceptionArgument Enum.");
                    return "";
            }
        }

#if false // Reflection-based implementation does not work for CoreRT/ProjectN
        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(ExceptionResource resource)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionResource), resource),
                "The enum value is not defined, please check the ExceptionResource Enum.");

            return SR.GetResourceString(resource.ToString());
        }
#endif

        private static string GetResourceString(ExceptionResource resource)
        {
            switch (resource)
            {
                case ExceptionResource.ArgumentOutOfRange_Index:
                    return "Index was out of range. Must be non-negative and less than the size of the collection.";
                case ExceptionResource.ArgumentOutOfRange_IndexCount:
                    return "Index and count must refer to a location within the string.";
                case ExceptionResource.ArgumentOutOfRange_IndexCountBuffer:
                    return "Index and count must refer to a location within the buffer.";
                case ExceptionResource.ArgumentOutOfRange_Count:
                    return "Count must be positive and count must refer to a location within the string/array/collection.";
                case ExceptionResource.ArgumentOutOfRange_Year:
                    return "SR.ArgumentOutOfRange_Year";
                case ExceptionResource.Arg_ArrayPlusOffTooSmall:
                    return "Destination array is not long enough to copy all the items in the collection. Check array index and length.";
                case ExceptionResource.NotSupported_ReadOnlyCollection:
                    return "Collection is read-only.";
                case ExceptionResource.Arg_RankMultiDimNotSupported:
                    return "SR.Arg_RankMultiDimNotSupported";
                case ExceptionResource.Arg_NonZeroLowerBound:
                    return "The lower bound of target array must be zero.";
                case ExceptionResource.ArgumentOutOfRange_GetCharCountOverflow:
                    return "SR.ArgumentOutOfRange_GetCharCountOverflow";
                case ExceptionResource.ArgumentOutOfRange_ListInsert:
                    return "Index must be within the bounds of the List.";
                case ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
                    return "Non-negative number required.";
                case ExceptionResource.ArgumentOutOfRange_SmallCapacity:
                    return "capacity was less than the current size.";
                case ExceptionResource.Argument_InvalidOffLen:
                    return "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
                case ExceptionResource.Argument_CannotExtractScalar:
                    return "SR.Argument_CannotExtractScalar";
                case ExceptionResource.ArgumentOutOfRange_BiggerThanCollection:
                    return "Larger than collection size.";
                case ExceptionResource.Serialization_MissingKeys:
                    return "SR.Serialization_MissingKeys";
                case ExceptionResource.Serialization_NullKey:
                    return "SR.Serialization_NullKey";
                case ExceptionResource.NotSupported_KeyCollectionSet:
                    return "SR.NotSupported_KeyCollectionSet";
                case ExceptionResource.NotSupported_ValueCollectionSet:
                    return "SR.NotSupported_ValueCollectionSet";
                case ExceptionResource.InvalidOperation_NullArray:
                    return "SR.InvalidOperation_NullArray";
                case ExceptionResource.TaskT_TransitionToFinal_AlreadyCompleted:
                    return "SR.TaskT_TransitionToFinal_AlreadyCompleted";
                case ExceptionResource.TaskCompletionSourceT_TrySetException_NullException:
                    return "SR.TaskCompletionSourceT_TrySetException_NullException";
                case ExceptionResource.TaskCompletionSourceT_TrySetException_NoExceptions:
                    return "SR.TaskCompletionSourceT_TrySetException_NoExceptions";
                case ExceptionResource.NotSupported_StringComparison:
                    return "SR.NotSupported_StringComparison";
                case ExceptionResource.ConcurrentCollection_SyncRoot_NotSupported:
                    return "SR.ConcurrentCollection_SyncRoot_NotSupported";
                case ExceptionResource.Task_MultiTaskContinuation_NullTask:
                    return "SR.Task_MultiTaskContinuation_NullTask";
                case ExceptionResource.InvalidOperation_WrongAsyncResultOrEndCalledMultiple:
                    return "SR.InvalidOperation_WrongAsyncResultOrEndCalledMultiple";
                case ExceptionResource.Task_MultiTaskContinuation_EmptyTaskList:
                    return "SR.Task_MultiTaskContinuation_EmptyTaskList";
                case ExceptionResource.Task_Start_TaskCompleted:
                    return "SR.Task_Start_TaskCompleted";
                case ExceptionResource.Task_Start_Promise:
                    return "SR.Task_Start_Promise";
                case ExceptionResource.Task_Start_ContinuationTask:
                    return "SR.Task_Start_ContinuationTask";
                case ExceptionResource.Task_Start_AlreadyStarted:
                    return "SR.Task_Start_AlreadyStarted";
                case ExceptionResource.Task_RunSynchronously_Continuation:
                    return "SR.Task_RunSynchronously_Continuation";
                case ExceptionResource.Task_RunSynchronously_Promise:
                    return "SR.Task_RunSynchronously_Promise";
                case ExceptionResource.Task_RunSynchronously_TaskCompleted:
                    return "SR.Task_RunSynchronously_TaskCompleted";
                case ExceptionResource.Task_RunSynchronously_AlreadyStarted:
                    return "SR.Task_RunSynchronously_AlreadyStarted";
                case ExceptionResource.AsyncMethodBuilder_InstanceNotInitialized:
                    return "SR.AsyncMethodBuilder_InstanceNotInitialized";
                case ExceptionResource.Task_ContinueWith_ESandLR:
                    return "SR.Task_ContinueWith_ESandLR";
                case ExceptionResource.Task_ContinueWith_NotOnAnything:
                    return "SR.Task_ContinueWith_NotOnAnything";
                case ExceptionResource.Task_InvalidTimerTimeSpan:
                    return "SR.Task_InvalidTimerTimeSpan";
                case ExceptionResource.Task_Delay_InvalidMillisecondsDelay:
                    return "SR.Task_Delay_InvalidMillisecondsDelay";
                case ExceptionResource.Task_Dispose_NotCompleted:
                    return "SR.Task_Dispose_NotCompleted";
                case ExceptionResource.Task_ThrowIfDisposed:
                    return "SR.Task_ThrowIfDisposed";
                case ExceptionResource.Task_WaitMulti_NullTask:
                    return "SR.Task_WaitMulti_NullTask";
                case ExceptionResource.ArgumentException_OtherNotArrayOfCorrectLength:
                    return "SR.ArgumentException_OtherNotArrayOfCorrectLength";
                case ExceptionResource.ArgumentNull_Array:
                    return "Array cannot be null.";
                case ExceptionResource.ArgumentNull_SafeHandle:
                    return "SR.ArgumentNull_SafeHandle";
                case ExceptionResource.ArgumentOutOfRange_EndIndexStartIndex:
                    return "endIndex cannot be greater than startIndex.";
                case ExceptionResource.ArgumentOutOfRange_Enum:
                    return "Enum value was out of legal range.";
                case ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported:
                    return "Arrays larger than 2GB are not supported.";
                case ExceptionResource.Argument_AddingDuplicate:
                    return "An item with the same key has already been added.";
                case ExceptionResource.Argument_InvalidArgumentForComparison:
                    return "SR.Argument_InvalidArgumentForComparison";
                case ExceptionResource.Arg_LowerBoundsMustMatch:
                    return "SR.Arg_LowerBoundsMustMatch";
                case ExceptionResource.Arg_MustBeType:
                    return "SR.Arg_MustBeType";
                case ExceptionResource.Arg_Need1DArray:
                    return "SR.Arg_Need1DArray";
                case ExceptionResource.Arg_Need2DArray:
                    return "SR.Arg_Need2DArray";
                case ExceptionResource.Arg_Need3DArray:
                    return "SR.Arg_Need3DArray";
                case ExceptionResource.Arg_NeedAtLeast1Rank:
                    return "SR.Arg_NeedAtLeast1Rank";
                case ExceptionResource.Arg_RankIndices:
                    return "SR.Arg_RankIndices";
                case ExceptionResource.Arg_RanksAndBounds:
                    return "SR.Arg_RanksAndBounds";
                case ExceptionResource.InvalidOperation_IComparerFailed:
                    return "SR.InvalidOperation_IComparerFailed";
                case ExceptionResource.NotSupported_FixedSizeCollection:
                    return "SR.NotSupported_FixedSizeCollection";
                case ExceptionResource.Rank_MultiDimNotSupported:
                    return "SR.Rank_MultiDimNotSupported";
                case ExceptionResource.Arg_TypeNotSupported:
                    return "SR.Arg_TypeNotSupported";
                case ExceptionResource.Argument_SpansMustHaveSameLength:
                    return "Length of items must be same as length of keys.";
                case ExceptionResource.Argument_InvalidFlag:
                    return "Value of flags is invalid.";
                case ExceptionResource.CancellationTokenSource_Disposed:
                    return "SR.CancellationTokenSource_Disposed";
                case ExceptionResource.Argument_AlignmentMustBePow2:
                    return "The alignment must be a power of two.";
                default:
                    Debug.Fail("The enum value is not defined, please check the ExceptionResource Enum.");
                    return "";
            }
        }
    }

    //
    // The convention for this enum is using the argument name as the enum name
    //
    internal enum ExceptionArgument
    {
        obj,
        dictionary,
        array,
        info,
        key,
        text,
        values,
        value,
        startIndex,
        task,
        bytes,
        byteIndex,
        byteCount,
        ch,
        chars,
        charIndex,
        charCount,
        s,
        input,
        ownedMemory,
        list,
        index,
        capacity,
        collection,
        item,
        converter,
        match,
        count,
        action,
        comparison,
        exceptions,
        exception,
        pointer,
        start,
        format,
        formats,
        culture,
        comparer,
        comparable,
        source,
        state,
        length,
        comparisonType,
        manager,
        sourceBytesToCopy,
        callBack,
        creationOptions,
        function,
        scheduler,
        continuationAction,
        continuationFunction,
        tasks,
        asyncResult,
        beginMethod,
        endMethod,
        endFunction,
        cancellationToken,
        continuationOptions,
        delay,
        millisecondsDelay,
        millisecondsTimeout,
        stateMachine,
        timeout,
        type,
        sourceIndex,
        sourceArray,
        destinationIndex,
        destinationArray,
        pHandle,
        handle,
        other,
        newSize,
        lowerBounds,
        lengths,
        len,
        keys,
        indices,
        index1,
        index2,
        index3,
        length1,
        length2,
        length3,
        endIndex,
        elementType,
        arrayIndex,
        year,
        codePoint,
        str,
        options,
        prefix,
        suffix,
        buffer,
        buffers,
        offset,
        stream
    }

    //
    // The convention for this enum is using the resource name as the enum name
    //
    internal enum ExceptionResource
    {
        ArgumentOutOfRange_Index,
        ArgumentOutOfRange_IndexCount,
        ArgumentOutOfRange_IndexCountBuffer,
        ArgumentOutOfRange_Count,
        ArgumentOutOfRange_Year,
        Arg_ArrayPlusOffTooSmall,
        NotSupported_ReadOnlyCollection,
        Arg_RankMultiDimNotSupported,
        Arg_NonZeroLowerBound,
        ArgumentOutOfRange_GetCharCountOverflow,
        ArgumentOutOfRange_ListInsert,
        ArgumentOutOfRange_NeedNonNegNum,
        ArgumentOutOfRange_SmallCapacity,
        Argument_InvalidOffLen,
        Argument_CannotExtractScalar,
        ArgumentOutOfRange_BiggerThanCollection,
        Serialization_MissingKeys,
        Serialization_NullKey,
        NotSupported_KeyCollectionSet,
        NotSupported_ValueCollectionSet,
        InvalidOperation_NullArray,
        TaskT_TransitionToFinal_AlreadyCompleted,
        TaskCompletionSourceT_TrySetException_NullException,
        TaskCompletionSourceT_TrySetException_NoExceptions,
        NotSupported_StringComparison,
        ConcurrentCollection_SyncRoot_NotSupported,
        Task_MultiTaskContinuation_NullTask,
        InvalidOperation_WrongAsyncResultOrEndCalledMultiple,
        Task_MultiTaskContinuation_EmptyTaskList,
        Task_Start_TaskCompleted,
        Task_Start_Promise,
        Task_Start_ContinuationTask,
        Task_Start_AlreadyStarted,
        Task_RunSynchronously_Continuation,
        Task_RunSynchronously_Promise,
        Task_RunSynchronously_TaskCompleted,
        Task_RunSynchronously_AlreadyStarted,
        AsyncMethodBuilder_InstanceNotInitialized,
        Task_ContinueWith_ESandLR,
        Task_ContinueWith_NotOnAnything,
        Task_InvalidTimerTimeSpan,
        Task_Delay_InvalidMillisecondsDelay,
        Task_Dispose_NotCompleted,
        Task_ThrowIfDisposed,
        Task_WaitMulti_NullTask,
        ArgumentException_OtherNotArrayOfCorrectLength,
        ArgumentNull_Array,
        ArgumentNull_SafeHandle,
        ArgumentOutOfRange_EndIndexStartIndex,
        ArgumentOutOfRange_Enum,
        ArgumentOutOfRange_HugeArrayNotSupported,
        Argument_AddingDuplicate,
        Argument_InvalidArgumentForComparison,
        Arg_LowerBoundsMustMatch,
        Arg_MustBeType,
        Arg_Need1DArray,
        Arg_Need2DArray,
        Arg_Need3DArray,
        Arg_NeedAtLeast1Rank,
        Arg_RankIndices,
        Arg_RanksAndBounds,
        InvalidOperation_IComparerFailed,
        NotSupported_FixedSizeCollection,
        Rank_MultiDimNotSupported,
        Arg_TypeNotSupported,
        Argument_SpansMustHaveSameLength,
        Argument_InvalidFlag,
        CancellationTokenSource_Disposed,
        Argument_AlignmentMustBePow2,
    }
}

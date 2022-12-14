// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Threading
{
    internal sealed partial class PortableThreadPool
    {
        /// <summary>
        /// Tracks information on the number of threads we want/have in different states in our thread pool.
        /// </summary>
        private struct ThreadCounts
        {
            // SOS's ThreadPool command depends on this layout
            private const byte NumProcessingWorkShift = 0;
            private const byte NumExistingThreadsShift = 16;
            private const byte NumThreadsGoalShift = 32;

            private ulong _data; // SOS's ThreadPool command depends on this name

            private ThreadCounts(ulong data) => _data = data;

            private short GetInt16Value(byte shift) => (short)(_data >> shift);
            private void SetInt16Value(short value, byte shift) =>
                _data = (_data & ~((ulong)ushort.MaxValue << shift)) | ((ulong)(ushort)value << shift);

            /// <summary>
            /// Number of threads processing work items.
            /// </summary>
            public short NumProcessingWork
            {
                get
                {
                    short value = GetInt16Value(NumProcessingWorkShift);
                    Debug.Assert(value >= 0);
                    return value;
                }
                set
                {
                    Debug.Assert(value >= 0);
                    SetInt16Value(Math.Max((short)0, value), NumProcessingWorkShift);
                }
            }

            /// <summary>
            /// Number of thread pool threads that currently exist.
            /// </summary>
            public short NumExistingThreads
            {
                get
                {
                    short value = GetInt16Value(NumExistingThreadsShift);
                    Debug.Assert(value >= 0);
                    return value;
                }
                set
                {
                    Debug.Assert(value >= 0);
                    SetInt16Value(Math.Max((short)0, value), NumExistingThreadsShift);
                }
            }

            /// <summary>
            /// Max possible thread pool threads we want to have.
            /// </summary>
            public short NumThreadsGoal
            {
                get
                {
                    short value = GetInt16Value(NumThreadsGoalShift);
                    Debug.Assert(value > 0);
                    return value;
                }
                set
                {
                    Debug.Assert(value > 0);
                    SetInt16Value(Math.Max((short)1, value), NumThreadsGoalShift);
                }
            }

            public ThreadCounts InterlockedSetNumThreadsGoal(short value)
            {
                ThreadPoolInstance._threadAdjustmentLock.VerifyIsLocked();

                ThreadCounts counts = this;
                while (true)
                {
                    ThreadCounts newCounts = counts;
                    newCounts.NumThreadsGoal = value;

                    ThreadCounts countsBeforeUpdate = InterlockedCompareExchange(newCounts, counts);
                    if (countsBeforeUpdate == counts)
                    {
                        return newCounts;
                    }

                    counts = countsBeforeUpdate;
                }
            }

            public ThreadCounts VolatileRead() => new ThreadCounts(Volatile.Read(ref _data));

            public ThreadCounts InterlockedCompareExchange(ThreadCounts newCounts, ThreadCounts oldCounts)
            {
#if DEBUG
                if (newCounts.NumThreadsGoal != oldCounts.NumThreadsGoal)
                {
                    ThreadPoolInstance._threadAdjustmentLock.VerifyIsLocked();
                }
#endif

                return new ThreadCounts(Interlocked.CompareExchange(ref _data, newCounts._data, oldCounts._data));
            }

            public static bool operator ==(ThreadCounts lhs, ThreadCounts rhs) => lhs._data == rhs._data;
            public static bool operator !=(ThreadCounts lhs, ThreadCounts rhs) => lhs._data != rhs._data;

            public override bool Equals([NotNullWhen(true)] object? obj) => obj is ThreadCounts other && _data == other._data;
            public override int GetHashCode() => (int)_data + (int)(_data >> 32);
        }
    }
}

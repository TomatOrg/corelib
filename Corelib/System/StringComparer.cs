// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System
{
    public abstract class StringComparer : IComparer<string?>, IEqualityComparer<string?>
    {
        // public static StringComparer InvariantCulture => CultureAwareComparer.InvariantCaseSensitiveInstance;
        //
        // public static StringComparer InvariantCultureIgnoreCase => CultureAwareComparer.InvariantIgnoreCaseInstance;

        // public static StringComparer CurrentCulture =>
        //     new CultureAwareComparer(CultureInfo.CurrentCulture, CompareOptions.None);
        //
        // public static StringComparer CurrentCultureIgnoreCase =>
        //     new CultureAwareComparer(CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);

        public static StringComparer Ordinal => OrdinalCaseSensitiveComparer.Instance;

        public static StringComparer OrdinalIgnoreCase => OrdinalIgnoreCaseComparer.Instance;

        // Convert a StringComparison to a StringComparer
        public static StringComparer FromComparison(StringComparison comparisonType)
        {
            return comparisonType switch
            {
                // StringComparison.CurrentCulture => CurrentCulture,
                // StringComparison.CurrentCultureIgnoreCase => CurrentCultureIgnoreCase,
                // StringComparison.InvariantCulture => InvariantCulture,
                // StringComparison.InvariantCultureIgnoreCase => InvariantCultureIgnoreCase,
                StringComparison.Ordinal => Ordinal,
                StringComparison.OrdinalIgnoreCase => OrdinalIgnoreCase,
                _ => throw new ArgumentException(ArgumentException.StringComparison, nameof(comparisonType)),
            };
        }

        // public static StringComparer Create(CultureInfo culture, bool ignoreCase)
        // {
        //     if (culture == null)
        //     {
        //         throw new ArgumentNullException(nameof(culture));
        //     }
        //
        //     return new CultureAwareComparer(culture, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        // }

        // public static StringComparer Create(CultureInfo culture, CompareOptions options)
        // {
        //     if (culture == null)
        //     {
        //         throw new ArgumentNullException(nameof(culture));
        //     }
        //
        //     return new CultureAwareComparer(culture, options);
        // }

        /// <summary>
        /// Determines whether the specified <see cref="IEqualityComparer{String}"/> is a well-known ordinal string comparer.
        /// </summary>
        /// <param name="comparer">The comparer to query.</param>
        /// <param name="ignoreCase">When this method returns, contains a value stating whether <paramref name="comparer"/>
        /// is case-insensitive. Set to <see langword="false"/> if this method returns <see langword="false"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="comparer"/> is a well-known ordinal string comparer;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A "well-known ordinal comparer" describes a comparer which behaves identically to <see cref="Ordinal"/>
        /// when passed to <see cref="Dictionary{String, TValue}.Dictionary"/> or <see cref="HashSet{String}.HashSet"/>.
        /// For example, <see cref="EqualityComparer{String}.Default"/> is a well-known ordinal comparer because
        /// a <see cref="Dictionary{String, TValue}"/> given <see cref="EqualityComparer{String}.Default"/> as a constructor
        /// argument will behave identically to a <see cref="Dictionary{String, TValue}"/> given <see cref="Ordinal"/>
        /// as a constructor argument. If <paramref name="ignoreCase"/> is <see langword="true"/> on method exit,
        /// then <paramref name="comparer"/> behaves identically to <see cref="OrdinalIgnoreCase"/> when passed to the
        /// constructor of such a collection.
        /// </remarks>
        public static bool IsWellKnownOrdinalComparer(IEqualityComparer<string?>? comparer, out bool ignoreCase)
        {
            if (comparer is IInternalStringEqualityComparer internalStringComparer)
            {
                comparer = internalStringComparer.GetUnderlyingEqualityComparer(); // unwrap if necessary
            }

            switch (comparer)
            {
                case StringComparer stringComparer:
                    return stringComparer.IsWellKnownOrdinalComparerCore(out ignoreCase);
                case GenericEqualityComparer<string>:
                    // special-case EqualityComparer<string>.Default, which is Ordinal-equivalent
                    ignoreCase = false;
                    return true;
                default:
                    // unknown comparer
                    ignoreCase = default;
                    return false;
            }
        }

        private protected virtual bool IsWellKnownOrdinalComparerCore(out bool ignoreCase)
        {
            // unless specialized comparer overrides this, we're not a well-known ordinal comparer
            ignoreCase = default;
            return false;
        }

        // /// <summary>
        // /// Determines whether the specified <see cref="IEqualityComparer{String}"/> is a well-known culture-aware string comparer.
        // /// </summary>
        // /// <param name="comparer">The comparer to query.</param>
        // /// <param name="compareInfo">When this method returns, contains a value indicating which <see cref="CompareInfo"/> was used
        // /// to create <paramref name="comparer"/>. Set to <see langword="null"/> if this method returns <see langword="false"/>.</param>
        // /// <param name="compareOptions">When this method returns, contains a value indicating which <see cref="CompareOptions"/> was used
        // /// to create <paramref name="comparer"/>. Set to <see cref="CompareOptions.None"/> if this method returns <see langword="false"/>.</param>
        // /// whether <paramref name="comparer"/>
        // /// <returns>
        // /// <see langword="true"/> if <paramref name="comparer"/> is a well-known culture-aware string comparer;
        // /// otherwise, <see langword="false"/>.
        // /// </returns>
        // /// <remarks>
        // /// A "well-known culture-aware comparer" describes a comparer which is tied to a specific <see cref="CompareInfo"/> using
        // /// some defined <see cref="CompareOptions"/>. To create a <see cref="StringComparer"/> instance wrapped around a
        // /// <see cref="CompareInfo"/> and <see cref="CompareOptions"/>, use <see cref="GlobalizationExtensions.GetStringComparer(CompareInfo, CompareOptions)"/>.
        // /// This method returns <see langword="false"/> when given <see cref="Ordinal"/> and other non-linguistic comparers as input.
        // /// </remarks>
        // public static bool IsWellKnownCultureAwareComparer(IEqualityComparer<string?>? comparer, [NotNullWhen(true)] out CompareInfo? compareInfo, out CompareOptions compareOptions)
        // {
        //     if (comparer is IInternalStringEqualityComparer internalStringComparer)
        //     {
        //         comparer = internalStringComparer.GetUnderlyingEqualityComparer(); // unwrap if necessary
        //     }
        //
        //     if (comparer is StringComparer stringComparer)
        //     {
        //         return stringComparer.IsWellKnownCultureAwareComparerCore(out compareInfo, out compareOptions);
        //     }
        //     else
        //     {
        //         // unknown comparer
        //         compareInfo = default;
        //         compareOptions = default;
        //         return false;
        //     }
        // }

        // private protected virtual bool IsWellKnownCultureAwareComparerCore([NotNullWhen(true)] out CompareInfo? compareInfo, out CompareOptions compareOptions)
        // {
        //     // unless specialized comparer overrides this, we're not a well-known culture-aware comparer
        //     compareInfo = default;
        //     compareOptions = default;
        //     return false;
        // }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj is string s)
            {
                return GetHashCode(s);
            }
            return obj.GetHashCode();
        }

        public abstract int Compare(string? x, string? y);
        public abstract bool Equals(string? x, string? y);
#pragma warning disable CS8614 // Remove warning disable when nullable attributes are respected
        public abstract int GetHashCode(string obj);
#pragma warning restore CS8614
    }

    public class OrdinalComparer : StringComparer
    {
        private readonly bool _ignoreCase; // Do not rename (binary serialization)

        internal OrdinalComparer(bool ignoreCase)
        {
            _ignoreCase = ignoreCase;
        }

        public override int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            if (_ignoreCase)
            {
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
            }

            return string.CompareOrdinal(x, y);
        }

        public override bool Equals(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x == null || y == null)
                return false;

            if (_ignoreCase)
            {
                if (x.Length != y.Length)
                {
                    return false;
                }
                return System.Globalization.Ordinal.EqualsIgnoreCase(ref x.GetRawStringData(), ref y.GetRawStringData(), x.Length);
            }
            return x.Equals(y);
        }

        public override int GetHashCode(string obj)
        {
            if (obj == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.obj);
            }

            if (_ignoreCase)
            {
                return obj.GetHashCodeOrdinalIgnoreCase();
            }

            return obj.GetHashCode();
        }

        // Equals method for the comparer itself.
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (!(obj is OrdinalComparer comparer))
            {
                return false;
            }
            return this._ignoreCase == comparer._ignoreCase;
        }

        public override int GetHashCode()
        {
            int hashCode = nameof(OrdinalComparer).GetHashCode();
            return _ignoreCase ? (~hashCode) : hashCode;
        }

        private protected override bool IsWellKnownOrdinalComparerCore(out bool ignoreCase)
        {
            ignoreCase = _ignoreCase;
            return true;
        }
    }

    internal sealed class OrdinalCaseSensitiveComparer : OrdinalComparer
    {
        internal static readonly OrdinalCaseSensitiveComparer Instance = new OrdinalCaseSensitiveComparer();

        private OrdinalCaseSensitiveComparer() : base(false)
        {
        }

        public override int Compare(string? x, string? y) => string.CompareOrdinal(x, y);

        public override bool Equals(string? x, string? y) => string.Equals(x, y);

        public override int GetHashCode(string obj)
        {
            if (obj == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.obj);
            }
            return obj.GetHashCode();
        }
    }

    internal sealed class OrdinalIgnoreCaseComparer : OrdinalComparer
    {
        internal static readonly OrdinalIgnoreCaseComparer Instance = new OrdinalIgnoreCaseComparer();

        private OrdinalIgnoreCaseComparer() : base(true)
        {
        }

        public override int Compare(string? x, string? y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            if (x.Length != y.Length)
            {
                return false;
            }

            return System.Globalization.Ordinal.EqualsIgnoreCase(ref x.GetRawStringData(), ref y.GetRawStringData(), x.Length);
        }

        public override int GetHashCode(string obj)
        {
            if (obj == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.obj);
            }
            return obj.GetHashCodeOrdinalIgnoreCase();
        }
    }
}

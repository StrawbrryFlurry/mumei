#if NETSTANDARD2_0
// reSharper disable All

using Roslyn.Utilities;
using System.Collections.Immutable;
using System.Globalization;

namespace Microsoft.Interop {
    /// <summary>
    /// Exposes the hashing utilities from Roslyn
    /// </summary>
    internal static class HashCode {
        public static int Combine<T1, T2>(T1 t1, T2 t2) {
            return Hash.Combine(t1 != null ? t1.GetHashCode() : 0, t2 != null ? t2.GetHashCode() : 0);
        }

        public static int Combine<T1, T2, T3>(T1 t1, T2 t2, T3 t3) {
            var combinedHash = t1 != null ? t1.GetHashCode() : 0;
            combinedHash = Hash.Combine(combinedHash, t2 != null ? t2.GetHashCode() : 0);
            return Hash.Combine(combinedHash, t3 != null ? t3.GetHashCode() : 0);
        }

        public static int Combine<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4) {
            var combinedHash = t1 != null ? t1.GetHashCode() : 0;
            combinedHash = Hash.Combine(combinedHash, t2 != null ? t2.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t3 != null ? t3.GetHashCode() : 0);
            return Hash.Combine(combinedHash, t4 != null ? t4.GetHashCode() : 0);
        }

        public static int Combine<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) {
            var combinedHash = t1 != null ? t1.GetHashCode() : 0;
            combinedHash = Hash.Combine(combinedHash, t2 != null ? t2.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t3 != null ? t3.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t4 != null ? t4.GetHashCode() : 0);
            return Hash.Combine(combinedHash, t5 != null ? t5.GetHashCode() : 0);
        }

        public static int Combine<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) {
            var combinedHash = t1 != null ? t1.GetHashCode() : 0;
            combinedHash = Hash.Combine(combinedHash, t2 != null ? t2.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t3 != null ? t3.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t4 != null ? t4.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t5 != null ? t5.GetHashCode() : 0);
            return Hash.Combine(combinedHash, t6 != null ? t6.GetHashCode() : 0);
        }

        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) {
            var combinedHash = t1 != null ? t1.GetHashCode() : 0;
            combinedHash = Hash.Combine(combinedHash, t2 != null ? t2.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t3 != null ? t3.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t4 != null ? t4.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t5 != null ? t5.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t6 != null ? t6.GetHashCode() : 0);
            return Hash.Combine(combinedHash, t7 != null ? t7.GetHashCode() : 0);
        }

        public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) {
            var combinedHash = t1 != null ? t1.GetHashCode() : 0;
            combinedHash = Hash.Combine(combinedHash, t2 != null ? t2.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t3 != null ? t3.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t4 != null ? t4.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t5 != null ? t5.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t6 != null ? t6.GetHashCode() : 0);
            combinedHash = Hash.Combine(combinedHash, t7 != null ? t7.GetHashCode() : 0);
            return Hash.Combine(combinedHash, t8 != null ? t8.GetHashCode() : 0);
        }

        public static int SequentialValuesHash<T>(IEnumerable<T> values) {
            var hash = 0;
            foreach (var value in values) {
                hash = Hash.Combine(hash, value!.GetHashCode());
            }
            return hash;
        }
    }
}

namespace Roslyn.Utilities {
    internal static class Hash {
        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// </summary>
        internal static int Combine(int newKey, int currentKey) {
            return unchecked(currentKey * (int) 0xA5555529 + newKey);
        }

        internal static int Combine(bool newKeyPart, int currentKey) {
            return Combine(currentKey, newKeyPart ? 1 : 0);
        }

        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// PERF: Do not use with enum types because that involves multiple
        /// unnecessary boxing operations.  Unfortunately, we can't constrain
        /// T to "non-enum", so we'll use a more restrictive constraint.
        /// </summary>
        internal static int Combine<T>(T newKeyPart, int currentKey) where T : class? {
            var hash = unchecked(currentKey * (int) 0xA5555529);

            if (newKeyPart != null) {
                return unchecked(hash + newKeyPart.GetHashCode());
            }

            return hash;
        }

        internal static int CombineValues<T>(IEnumerable<T>? values, int maxItemsToHash = int.MaxValue) {
            if (values == null) {
                return 0;
            }

            var hashCode = 0;
            var count = 0;
            foreach (var value in values) {
                if (count++ >= maxItemsToHash) {
                    break;
                }

                // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                if (value != null) {
                    hashCode = Combine(value.GetHashCode(), hashCode);
                }
            }

            return hashCode;
        }

        internal static int CombineValues<TKey, TValue>(ImmutableDictionary<TKey, TValue> values, int maxItemsToHash = int.MaxValue)
            where TKey : notnull {
            if (values == null)
                return 0;

            var hashCode = 0;
            var count = 0;
            foreach (var value in values) {
                if (count++ >= maxItemsToHash)
                    break;

                hashCode = Combine(value.GetHashCode(), hashCode);
            }

            return hashCode;
        }

        internal static int CombineValues<T>(T[]? values, int maxItemsToHash = int.MaxValue) {
            if (values == null) {
                return 0;
            }

            var maxSize = Math.Min(maxItemsToHash, values.Length);
            var hashCode = 0;

            for (var i = 0; i < maxSize; i++) {
                var value = values[i];

                // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                if (value != null) {
                    hashCode = Combine(value.GetHashCode(), hashCode);
                }
            }

            return hashCode;
        }

        internal static int CombineValues<T>(ImmutableArray<T> values, int maxItemsToHash = int.MaxValue) {
            if (values.IsDefaultOrEmpty) {
                return 0;
            }

            var hashCode = 0;
            var count = 0;
            foreach (var value in values) {
                if (count++ >= maxItemsToHash) {
                    break;
                }

                // Should end up with a constrained virtual call to object.GetHashCode (i.e. avoid boxing where possible).
                if (value != null) {
                    hashCode = Combine(value.GetHashCode(), hashCode);
                }
            }

            return hashCode;
        }

        internal static int CombineValues(IEnumerable<string?>? values, StringComparer stringComparer, int maxItemsToHash = int.MaxValue) {
            if (values == null) {
                return 0;
            }

            var hashCode = 0;
            var count = 0;
            foreach (var value in values) {
                if (count++ >= maxItemsToHash) {
                    break;
                }

                if (value != null) {
                    hashCode = Combine(stringComparer.GetHashCode(value), hashCode);
                }
            }

            return hashCode;
        }

        internal static int CombineValues(ImmutableArray<string> values, StringComparer stringComparer, int maxItemsToHash = int.MaxValue) {
            if (values == null)
                return 0;

            var hashCode = 0;
            var count = 0;
            foreach (var value in values) {
                if (count++ >= maxItemsToHash)
                    break;

                if (value != null)
                    hashCode = Combine(stringComparer.GetHashCode(value), hashCode);
            }

            return hashCode;
        }

        /// <summary>
        /// The offset bias value used in the FNV-1a algorithm
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        internal const int FnvOffsetBias = unchecked((int) 2166136261);

        /// <summary>
        /// The generative factor used in the FNV-1a algorithm
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        internal const int FnvPrime = 16777619;

        /// <summary>
        /// Compute the FNV-1a hash of a sequence of bytes
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="data">The sequence of bytes</param>
        /// <returns>The FNV-1a hash of <paramref name="data"/></returns>
        internal static int GetFNVHashCode(byte[] data) {
            var hashCode = FnvOffsetBias;

            for (var i = 0; i < data.Length; i++) {
                hashCode = unchecked((hashCode ^ data[i]) * FnvPrime);
            }

            return hashCode;
        }

        /// <summary>
        /// Compute the FNV-1a hash of a sequence of bytes and determines if the byte
        /// sequence is valid ASCII and hence the hash code matches a char sequence
        /// encoding the same text.
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="data">The sequence of bytes that are likely to be ASCII text.</param>
        /// <param name="isAscii">True if the sequence contains only characters in the ASCII range.</param>
        /// <returns>The FNV-1a hash of <paramref name="data"/></returns>
        internal static int GetFNVHashCode(ReadOnlySpan<byte> data, out bool isAscii) {
            var hashCode = FnvOffsetBias;

            byte asciiMask = 0;

            for (var i = 0; i < data.Length; i++) {
                var b = data[i];
                asciiMask |= b;
                hashCode = unchecked((hashCode ^ b) * FnvPrime);
            }

            isAscii = (asciiMask & 0x80) == 0;
            return hashCode;
        }

        /// <summary>
        /// Compute the FNV-1a hash of a sequence of bytes
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="data">The sequence of bytes</param>
        /// <returns>The FNV-1a hash of <paramref name="data"/></returns>
        internal static int GetFNVHashCode(ImmutableArray<byte> data) {
            var hashCode = FnvOffsetBias;

            for (var i = 0; i < data.Length; i++) {
                hashCode = unchecked((hashCode ^ data[i]) * FnvPrime);
            }

            return hashCode;
        }

        /// <summary>
        /// Compute the hashcode of a sub-string using FNV-1a
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// Note: FNV-1a was developed and tuned for 8-bit sequences. We're using it here
        /// for 16-bit Unicode chars on the understanding that the majority of chars will
        /// fit into 8-bits and, therefore, the algorithm will retain its desirable traits
        /// for generating hash codes.
        /// </summary>
        internal static int GetFNVHashCode(ReadOnlySpan<char> data) {
            return CombineFNVHash(FnvOffsetBias, data);
        }

        /// <summary>
        /// Compute the hashcode of a sub-string using FNV-1a
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// Note: FNV-1a was developed and tuned for 8-bit sequences. We're using it here
        /// for 16-bit Unicode chars on the understanding that the majority of chars will
        /// fit into 8-bits and, therefore, the algorithm will retain its desirable traits
        /// for generating hash codes.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="start">The start index of the first character to hash</param>
        /// <param name="length">The number of characters, beginning with <paramref name="start"/> to hash</param>
        /// <returns>The FNV-1a hash code of the substring beginning at <paramref name="start"/> and ending after <paramref name="length"/> characters.</returns>
        internal static int GetFNVHashCode(string text, int start, int length) {
            return GetFNVHashCode(text.AsSpan(start, length));
        }

        internal static int GetCaseInsensitiveFNVHashCode(string text) {
            return GetCaseInsensitiveFNVHashCode(text.AsSpan());
        }

        internal static int GetCaseInsensitiveFNVHashCode(ReadOnlySpan<char> data) {
            var hashCode = FnvOffsetBias;

            for (var i = 0; i < data.Length; i++) {
                hashCode = unchecked((hashCode ^ CaseInsensitiveComparison.ToLower(data[i])) * FnvPrime);
            }

            return hashCode;
        }

        /// <summary>
        /// Compute the hashcode of a string using FNV-1a
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The FNV-1a hash code of <paramref name="text"/></returns>
        internal static int GetFNVHashCode(string text) {
            return CombineFNVHash(FnvOffsetBias, text);
        }

        /// <summary>
        /// Compute the hashcode of a string using FNV-1a
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The FNV-1a hash code of <paramref name="text"/></returns>
        internal static int GetFNVHashCode(System.Text.StringBuilder text) {
            var hashCode = FnvOffsetBias;

#if NETCOREAPP3_1_OR_GREATER
            foreach (var chunk in text.GetChunks())
            {
                hashCode = CombineFNVHash(hashCode, chunk.Span);
            }
#else
            // StringBuilder.GetChunks is not available in this target framework. Since there is no other direct access
            // to the underlying storage spans of StringBuilder, we fall back to using slower per-character operations.
            var end = text.Length;

            for (var i = 0; i < end; i++) {
                hashCode = unchecked((hashCode ^ text[i]) * FnvPrime);
            }
#endif

            return hashCode;
        }

        /// <summary>
        /// Compute the hashcode of a sub string using FNV-1a
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="text">The input string as a char array</param>
        /// <param name="start">The start index of the first character to hash</param>
        /// <param name="length">The number of characters, beginning with <paramref name="start"/> to hash</param>
        /// <returns>The FNV-1a hash code of the substring beginning at <paramref name="start"/> and ending after <paramref name="length"/> characters.</returns>
        internal static int GetFNVHashCode(char[] text, int start, int length) {
            return GetFNVHashCode(text.AsSpan(start, length));
        }

        /// <summary>
        /// Compute the hashcode of a single character using the FNV-1a algorithm
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// Note: In general, this isn't any more useful than "char.GetHashCode". However,
        /// it may be needed if you need to generate the same hash code as a string or
        /// substring with just a single character.
        /// </summary>
        /// <param name="ch">The character to hash</param>
        /// <returns>The FNV-1a hash code of the character.</returns>
        internal static int GetFNVHashCode(char ch) {
            return CombineFNVHash(FnvOffsetBias, ch);
        }

        /// <summary>
        /// Combine a string with an existing FNV-1a hash code
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="hashCode">The accumulated hash code</param>
        /// <param name="text">The string to combine</param>
        /// <returns>The result of combining <paramref name="hashCode"/> with <paramref name="text"/> using the FNV-1a algorithm</returns>
        internal static int CombineFNVHash(int hashCode, string text) {
            return CombineFNVHash(hashCode, text.AsSpan());
        }

        /// <summary>
        /// Combine a char with an existing FNV-1a hash code
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="hashCode">The accumulated hash code</param>
        /// <param name="ch">The new character to combine</param>
        /// <returns>The result of combining <paramref name="hashCode"/> with <paramref name="ch"/> using the FNV-1a algorithm</returns>
        internal static int CombineFNVHash(int hashCode, char ch) {
            return unchecked((hashCode ^ ch) * FnvPrime);
        }

        /// <summary>
        /// Combine a string with an existing FNV-1a hash code
        /// See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
        /// </summary>
        /// <param name="hashCode">The accumulated hash code</param>
        /// <param name="data">The string to combine</param>
        /// <returns>The result of combining <paramref name="hashCode"/> with <paramref name="data"/> using the FNV-1a algorithm</returns>
        internal static int CombineFNVHash(int hashCode, ReadOnlySpan<char> data) {
            for (var i = 0; i < data.Length; i++) {
                hashCode = unchecked((hashCode ^ data[i]) * FnvPrime);
            }

            return hashCode;
        }
    }

    static class CaseInsensitiveComparison {
        // PERF: Cache a TextInfo for Unicode ToLower since this will be accessed very frequently
        private static readonly TextInfo s_unicodeCultureTextInfo = GetUnicodeCulture().TextInfo;

        private static CultureInfo GetUnicodeCulture() {
            try {
                // We use the "en" culture to get the Unicode ToLower mapping, as it implements
                // a much more recent Unicode version (6.0+) than the invariant culture (1.0),
                // and it matches the Unicode version used for character categorization.
                return new CultureInfo("en");
            } catch (ArgumentException) // System.Globalization.CultureNotFoundException not on all platforms
            {
                // If "en" is not available, fall back to the invariant culture. Although it has bugs
                // specific to the invariant culture (e.g. being version-locked to Unicode 1.0), at least
                // we can rely on it being present on all platforms.
                return CultureInfo.InvariantCulture;
            }
        }

        /// <summary>
        /// ToLower implements the Unicode lowercase mapping
        /// as described in ftp://ftp.unicode.org/Public/UNIDATA/UnicodeData.txt.
        /// VB uses these mappings for case-insensitive comparison.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>If <paramref name="c"/> is upper case, then this returns its Unicode lower case equivalent. Otherwise, <paramref name="c"/> is returned unmodified.</returns>
        public static char ToLower(char c) {
            // PERF: This is a very hot code path in VB, optimize for ASCII

            // Perform a range check with a single compare by using unsigned arithmetic
            if (unchecked((uint) (c - 'A')) <= ('Z' - 'A')) {
                return (char) (c | 0x20);
            }

            if (c < 0xC0) // Covers ASCII (U+0000 - U+007F) and up to the next upper-case codepoint (Latin Capital Letter A with Grave)
            {
                return c;
            }

            return ToLowerNonAscii(c);
        }

        private static char ToLowerNonAscii(char c) {
            if (c == '\u0130') {
                // Special case Turkish I (LATIN CAPITAL LETTER I WITH DOT ABOVE)
                // This corrects for the fact that the invariant culture only supports Unicode 1.0
                // and therefore does not "know about" this character.
                return 'i';
            }

            return s_unicodeCultureTextInfo.ToLower(c);
        }
    }
}

#endif
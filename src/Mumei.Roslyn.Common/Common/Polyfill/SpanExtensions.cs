using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mumei.Roslyn.Common.Polyfill;

internal static class SpanExtensions {
    private const int MaxUInt32AsciiLength = 10;

    extension(char) {
        public static int MaxAsciiIntLength => MaxUInt32AsciiLength;

        public static int WriteIntAsAsciiChars(uint value, Span<char> target) {
            Debug.Assert(target.Length >= MaxUInt32AsciiLength);
            var index = 0;

            // > Number Formatting UInt32ToDecChars
            var intValue = (int) value;
            while (intValue >= 10) {
                intValue = Math.DivRem(intValue, 10, out var remainder);
                var digit = (char) ('0' + remainder);
                target[index] = digit;
                index++;
            }

            var finalDigit = (char) ('0' + intValue);
            target[index] = finalDigit;
            var charsWritten = index + 1;
            return charsWritten;
        }
    }

    extension(int) {
        public static bool TryParseAsciiInt(ReadOnlySpan<char> chars, out int result) {
            if (chars.IsEmpty) {
                result = -1;
                return false;
            }

            if (chars.Length == 1) {
                return TryParseSingleAsciiInt(chars[0], out result);
            }

            return TryParseAsciiIntSlow(chars, out result);
        }
    }

    private static bool TryParseSingleAsciiInt(char c, out int result) {
        if (!IsAsciiDigit(c)) {
            result = -1;
            return false;
        }

        result = AsciiDigitToIntFast(c);
        return true;
    }

    private static bool TryParseAsciiIntSlow(scoped ReadOnlySpan<char> chars, out int result) {
        result = 0;
        foreach (var c in chars) {
            if (!IsAsciiDigit(c)) {
                result = -1;
                return false;
            }

            result = result * 10 + AsciiDigitToIntFast(c);
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AsciiDigitToIntFast(char c) {
        return c - '0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAsciiDigit(char c) {
        return (uint) c - (uint) '0' <= (uint) '9' - (uint) '0';
    }
}
using System.Runtime.CompilerServices;

namespace Mumei.Roslyn.Common.Polyfill;

internal static class SpanExtensions {
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
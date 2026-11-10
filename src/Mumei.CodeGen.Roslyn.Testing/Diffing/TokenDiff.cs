namespace Mumei.CodeGen.Roslyn.Testing;

/// <summary>A range of characters within a single line.</summary>
public readonly record struct SourceTextRange(int Start, int Length) {
    public int End => Start + Length;
}

/// <summary>
/// Determines which parts of a changed line differ. Lines are split into identifiers / numbers, whitespace runs and
/// single punctuation characters, which reads much better for code than a character based diff
/// (<c>int</c> → <c>long</c> is reported as a whole instead of <c>i[n]t</c> → <c>lo[n]g</c>).
/// </summary>
internal static class TokenDiff {
    public readonly record struct Result(double Similarity, SourceTextRange[] Expected, SourceTextRange[] Actual);

    public static Result Compute(
        string expected,
        string actual,
        WhitespaceHandling whitespace,
        bool matchAnyPrefix = false,
        bool matchAnySuffix = false
    ) {
        var expectedTokens = Tokenize(expected, whitespace, parsePlaceholders: true, matchAnyPrefix, matchAnySuffix);
        var actualTokens = Tokenize(actual, whitespace, parsePlaceholders: false, false, false);

        var steps = SequenceAligner.Align(
            expectedTokens.Count,
            actualTokens.Count,
            (e, a) => TokensEqual(expected, expectedTokens[e], actual, actualTokens[a], whitespace),
            e => expectedTokens[e].Kind == TokenKind.Placeholder,
            partial: false
        );

        var expectedDiffs = new List<int>();
        var actualDiffs = new List<int>();
        int matched = 0, total = 0;
        foreach (var step in steps) {
            switch (step.Op) {
                case AlignmentOp.Match:
                    matched += 2 * Weight(expectedTokens[step.Expected]);
                    total += 2 * Weight(expectedTokens[step.Expected]);
                    break;
                case AlignmentOp.Missing:
                    expectedDiffs.Add(step.Expected);
                    total += Weight(expectedTokens[step.Expected]);
                    break;
                case AlignmentOp.Unexpected:
                    actualDiffs.Add(step.Actual);
                    total += Weight(actualTokens[step.Actual]);
                    break;
            }
        }

        var similarity = total == 0 ? 1d : (double)matched / total;
        return new Result(similarity, ToRanges(expectedTokens, expectedDiffs), ToRanges(actualTokens, actualDiffs));

        static int Weight(Token token) {
            return token.Kind == TokenKind.Whitespace ? 0 : token.Length;
        }
    }

    private static bool TokensEqual(string expected, Token e, string actual, Token a, WhitespaceHandling whitespace) {
        if (e.Kind != a.Kind) {
            return false;
        }

        if (e.Kind == TokenKind.Whitespace && whitespace == WhitespaceHandling.Normalize) {
            return true;
        }

        return expected.AsSpan(e.Start, e.Length).SequenceEqual(actual.AsSpan(a.Start, a.Length));
    }

    /// <summary>Merges differing tokens that are only separated by whitespace into a single range.</summary>
    private static SourceTextRange[] ToRanges(List<Token> tokens, List<int> diffIndices) {
        var ranges = new List<SourceTextRange>();
        var lastIndex = -1;
        foreach (var index in diffIndices) {
            var token = tokens[index];
            if (token.Length == 0) {
                continue;
            }

            var canMerge = ranges.Count > 0;
            for (var i = lastIndex + 1; canMerge && i < index; i++) {
                canMerge = tokens[i].Kind == TokenKind.Whitespace;
            }

            if (canMerge) {
                var previous = ranges[^1];
                ranges[^1] = new SourceTextRange(previous.Start, token.Start + token.Length - previous.Start);
            }
            else {
                ranges.Add(new SourceTextRange(token.Start, token.Length));
            }

            lastIndex = index;
        }

        return ranges.ToArray();
    }

    private static List<Token> Tokenize(
        string line,
        WhitespaceHandling whitespace,
        bool parsePlaceholders,
        bool matchAnyPrefix,
        bool matchAnySuffix
    ) {
        int start = 0, end = line.Length;
        if (whitespace != WhitespaceHandling.Exact) {
            while (start < end && char.IsWhiteSpace(line[start])) {
                start++;
            }

            while (end > start && char.IsWhiteSpace(line[end - 1])) {
                end--;
            }
        }

        var tokens = new List<Token>();
        if (matchAnyPrefix) {
            tokens.Add(new Token(start, 0, TokenKind.Placeholder));
        }

        var position = start;
        while (position < end) {
            var c = line[position];
            var tokenStart = position;
            TokenKind kind;
            if (parsePlaceholders && string.CompareOrdinal(line, position, LinePattern.AnyText, 0, LinePattern.AnyText.Length) == 0) {
                kind = TokenKind.Placeholder;
                position += LinePattern.AnyText.Length;
            }
            else if (char.IsWhiteSpace(c)) {
                kind = TokenKind.Whitespace;
                while (position < end && char.IsWhiteSpace(line[position])) {
                    position++;
                }
            }
            else if (char.IsLetterOrDigit(c) || c == '_') {
                kind = TokenKind.Word;
                while (position < end && (char.IsLetterOrDigit(line[position]) || line[position] == '_')) {
                    position++;
                }
            }
            else {
                kind = TokenKind.Punctuation;
                position++;
            }

            tokens.Add(new Token(tokenStart, position - tokenStart, kind));
        }

        if (matchAnySuffix) {
            tokens.Add(new Token(end, 0, TokenKind.Placeholder));
        }

        return tokens;
    }

    private enum TokenKind : byte {
        Word,
        Whitespace,
        Punctuation,
        Placeholder
    }

    private readonly record struct Token(int Start, int Length, TokenKind Kind);
}

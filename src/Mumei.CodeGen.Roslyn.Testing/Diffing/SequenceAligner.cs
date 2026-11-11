namespace Mumei.CodeGen.Roslyn.Testing.Diffing;

internal enum AlignmentOp : byte {
    None,
    Match,

    /// <summary>The expected item has no counterpart in the actual sequence.</summary>
    Missing,

    /// <summary>The actual item has no counterpart in the expected sequence.</summary>
    Unexpected,

    /// <summary>An actual item consumed by an expected multi-item wildcard.</summary>
    WildcardAbsorb,

    /// <summary>An expected multi-item wildcard that consumed nothing.</summary>
    WildcardEmpty,

    /// <summary>An actual item outside of the aligned region (partial alignments only).</summary>
    Outside
}

internal readonly record struct AlignmentStep(AlignmentOp Op, int Expected, int Actual);

/// <summary>
/// Aligns an expected sequence against an actual sequence, minimizing <c>missing + unexpected - matches</c>.
/// For full alignments this is equivalent to a longest-common-subsequence diff. For partial alignments
/// the expected sequence may start and end anywhere in the actual sequence (semi-global alignment); rewarding
/// matches makes the alignment prefer the region that resembles the expected sequence the most.
/// </summary>
internal static class SequenceAligner {
    private const long MaxCells = 64L * 1024 * 1024;

    public static List<AlignmentStep> Align(
        int expectedCount,
        int actualCount,
        Func<int, int, bool> matches,
        Func<int, bool> isWildcard,
        bool partial
    ) {
        var hasWildcards = false;
        for (var i = 0; i < expectedCount && !hasWildcards; i++) {
            hasWildcards = isWildcard(i);
        }

        // Trimming equal prefixes / suffixes is always optimal for plain LCS and keeps the DP small for
        // the common case of a few changed lines in a large file.
        int prefix = 0, suffix = 0;
        if (!partial && !hasWildcards) {
            while (prefix < expectedCount && prefix < actualCount && matches(prefix, prefix)) {
                prefix++;
            }

            while (suffix < expectedCount - prefix && suffix < actualCount - prefix
                   && matches(expectedCount - 1 - suffix, actualCount - 1 - suffix)) {
                suffix++;
            }
        }

        var steps = new List<AlignmentStep>(Math.Max(expectedCount, actualCount));
        for (var i = 0; i < prefix; i++) {
            steps.Add(new AlignmentStep(AlignmentOp.Match, i, i));
        }

        var m = expectedCount - prefix - suffix;
        var n = actualCount - prefix - suffix;
        if ((long)(m + 1) * (n + 1) > MaxCells) {
            AppendFallback(steps, prefix, m, n, partial, isWildcard);
        }
        else {
            AlignCore(steps, prefix, m, n, matches, isWildcard, partial);
        }

        for (var i = 0; i < suffix; i++) {
            steps.Add(new AlignmentStep(AlignmentOp.Match, expectedCount - suffix + i, actualCount - suffix + i));
        }

        return steps;
    }

    private static void AlignCore(
        List<AlignmentStep> steps,
        int offset,
        int m,
        int n,
        Func<int, int, bool> matches,
        Func<int, bool> isWildcard,
        bool partial
    ) {
        var width = n + 1;
        var directions = new AlignmentOp[(m + 1) * width];
        var previousScore = new int[width];
        var previousMatches = new int[width];
        var currentScore = new int[width];
        var currentMatches = new int[width];

        for (var j = 1; j <= n; j++) {
            previousScore[j] = partial ? 0 : j;
        }

        for (var i = 1; i <= m; i++) {
            var wildcard = isWildcard(offset + i - 1);
            var row = i * width;

            currentScore[0] = previousScore[0] + (wildcard ? 0 : 1);
            currentMatches[0] = previousMatches[0];
            directions[row] = wildcard ? AlignmentOp.WildcardEmpty : AlignmentOp.Missing;

            for (var j = 1; j <= n; j++) {
                var bestScore = int.MaxValue;
                var bestMatches = 0;
                var bestOp = AlignmentOp.None;

                if (wildcard) {
                    Consider(currentScore[j - 1], currentMatches[j - 1], AlignmentOp.WildcardAbsorb);
                    Consider(previousScore[j], previousMatches[j], AlignmentOp.WildcardEmpty);
                }
                else {
                    if (matches(offset + i - 1, offset + j - 1)) {
                        Consider(previousScore[j - 1] - 1, previousMatches[j - 1] + 1, AlignmentOp.Match);
                    }

                    Consider(previousScore[j] + 1, previousMatches[j], AlignmentOp.Missing);
                }

                Consider(currentScore[j - 1] + 1, currentMatches[j - 1], AlignmentOp.Unexpected);

                currentScore[j] = bestScore;
                currentMatches[j] = bestMatches;
                directions[row + j] = bestOp;

                void Consider(int score, int matchCount, AlignmentOp op) {
                    if (score < bestScore || (score == bestScore && matchCount > bestMatches)) {
                        bestScore = score;
                        bestMatches = matchCount;
                        bestOp = op;
                    }
                }
            }

            (previousScore, currentScore) = (currentScore, previousScore);
            (previousMatches, currentMatches) = (currentMatches, previousMatches);
        }

        // Partial alignments may end anywhere; prefer the best scoring, earliest end.
        var end = n;
        if (partial) {
            end = 0;
            for (var j = 1; j <= n; j++) {
                if (previousScore[j] < previousScore[end]
                    || (previousScore[j] == previousScore[end] && previousMatches[j] > previousMatches[end])) {
                    end = j;
                }
            }
        }

        var reversed = new List<AlignmentStep>(m + n);
        for (var j = n; j > end; j--) {
            reversed.Add(new AlignmentStep(AlignmentOp.Outside, -1, offset + j - 1));
        }

        int x = m, y = end;
        while (x > 0 || y > 0) {
            if (x == 0) {
                reversed.Add(new AlignmentStep(partial ? AlignmentOp.Outside : AlignmentOp.Unexpected, -1, offset + y - 1));
                y--;
                continue;
            }

            var op = directions[x * width + y];
            switch (op) {
                case AlignmentOp.Match:
                case AlignmentOp.WildcardAbsorb:
                    reversed.Add(new AlignmentStep(op, offset + x - 1, offset + y - 1));
                    if (op == AlignmentOp.Match) {
                        x--;
                    }

                    y--;
                    break;
                case AlignmentOp.Missing:
                case AlignmentOp.WildcardEmpty:
                    // A wildcard that absorbed lines ends its run with an "empty" step which carries no information.
                    var absorbedLines = op == AlignmentOp.WildcardEmpty && reversed.Count > 0
                                        && reversed[^1].Op == AlignmentOp.WildcardAbsorb && reversed[^1].Expected == offset + x - 1;
                    if (!absorbedLines) {
                        reversed.Add(new AlignmentStep(op, offset + x - 1, -1));
                    }

                    x--;
                    break;
                case AlignmentOp.Unexpected:
                    reversed.Add(new AlignmentStep(op, -1, offset + y - 1));
                    y--;
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected alignment direction {op} at ({x}, {y}).");
            }
        }

        reversed.Reverse();
        steps.AddRange(reversed);
    }

    private static void AppendFallback(List<AlignmentStep> steps, int offset, int m, int n, bool partial, Func<int, bool> isWildcard) {
        for (var i = 0; i < m; i++) {
            var op = isWildcard(offset + i) ? AlignmentOp.WildcardEmpty : AlignmentOp.Missing;
            steps.Add(new AlignmentStep(op, offset + i, -1));
        }

        for (var j = 0; j < n; j++) {
            steps.Add(new AlignmentStep(partial ? AlignmentOp.Outside : AlignmentOp.Unexpected, -1, offset + j));
        }
    }
}

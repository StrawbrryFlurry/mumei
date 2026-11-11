namespace Mumei.CodeGen.Roslyn.Testing.Diffing;

/// <summary>
/// Compares source texts line by line and reports differences with line numbers and the exact changed text within
/// each line. Expected texts may contain placeholders which are valid C# comments:
/// <list type="bullet">
/// <item><c>/*...*/</c> within a line matches any text in that line.</item>
/// <item>A line consisting only of <c>// ...</c> (or <c>/*...*/</c>) matches any number of lines, including none.</item>
/// </list>
/// </summary>
public static class SourceDiff {
    private const double MinSimilarityForChangedLine = 0.5;
    private const int MaxChangedLineLookahead = 32;

    /// <summary>Compares the entire <paramref name="actual"/> text against <paramref name="expected"/>.</summary>
    public static SourceDiffResult Compare(string expected, string actual, SourceComparisonOptions? options = null) {
        return Run(expected, actual, options ?? SourceComparisonOptions.Default, isFragment: false);
    }

    /// <summary>
    /// Checks that <paramref name="actual"/> contains <paramref name="expectedFragment"/> as a contiguous block of lines.
    /// The first line of the fragment may match the end of a line and the last line the start of a line, so a
    /// single line fragment matches anywhere within a line. If the fragment is not found, the result describes
    /// the differences to the most similar region of <paramref name="actual"/>.
    /// </summary>
    public static SourceDiffResult CompareFragment(string expectedFragment, string actual, SourceComparisonOptions? options = null) {
        return Run(expectedFragment, actual, options ?? SourceComparisonOptions.Default, isFragment: true);
    }

    private static SourceDiffResult Run(string expected, string actual, SourceComparisonOptions options, bool isFragment) {
        var expectedLines = SourceLines.Create(expected, options);
        var actualLines = SourceLines.Create(actual, options);

        var count = expectedLines.Keys.Length;
        var patterns = new LinePattern[count];
        for (var i = 0; i < count; i++) {
            patterns[i] = LinePattern.Create(expectedLines.Keys[i], isFragment && i == 0, isFragment && i == count - 1);
        }

        var steps = SequenceAligner.Align(
            count,
            actualLines.Keys.Length,
            (e, a) => patterns[e].Matches(actualLines.Keys[a]),
            e => patterns[e].IsAnyLines,
            isFragment
        );

        var builder = new LineBuilder(expectedLines, actualLines, options, isFragment);
        if (isFragment) {
            steps = steps.Any(x => x.Op == AlignmentOp.Match)
                ? IncludeChangedLinesAroundFragment(builder, steps)
                : AlignAtMostSimilarLine(builder, patterns, actualLines.Keys.Length) ?? steps;
        }
        var missing = new List<int>();
        var unexpected = new List<int>();
        foreach (var step in steps) {
            switch (step.Op) {
                case AlignmentOp.Missing:
                    missing.Add(step.Expected);
                    continue;
                case AlignmentOp.Unexpected:
                    unexpected.Add(step.Actual);
                    continue;
            }

            builder.AddDifferences(missing, unexpected);
            builder.Add(step);
        }

        builder.AddDifferences(missing, unexpected);

        return new SourceDiffResult(
            expected,
            actual,
            expectedLines.All,
            actualLines.All,
            options,
            isFragment,
            builder.Lines
        );
    }

    /// <summary>
    /// A fragment alignment never pays for unexpected lines at its edges, so expected lines at the start or end of the
    /// fragment that were changed end up as "missing" while their counterparts are outside of the aligned region.
    /// Pull actual lines that resemble those missing lines into the region so they are reported as changed lines.
    /// </summary>
    private static List<AlignmentStep> IncludeChangedLinesAroundFragment(LineBuilder builder, List<AlignmentStep> steps) {
        var first = steps.FindIndex(x => x.Op != AlignmentOp.Outside);
        var last = steps.FindLastIndex(x => x.Op != AlignmentOp.Outside);

        var leadingMissing = 0;
        while (steps[first + leadingMissing].Op == AlignmentOp.Missing) {
            leadingMissing++;
        }

        var trailingMissing = 0;
        while (steps[last - trailingMissing].Op == AlignmentOp.Missing) {
            trailingMissing++;
        }

        if (leadingMissing > 0) {
            var candidateCount = Math.Min(first, 2 * leadingMissing + 2);
            var candidates = first - candidateCount;
            var alignment = AlignBySimilarity(builder, steps, first, leadingMissing, candidates, candidateCount);
            if (alignment.FirstOrDefault(x => x.Op == AlignmentOp.Match) is { Op: AlignmentOp.Match } firstMatch) {
                MarkUnexpected(steps, candidates + firstMatch.Actual, first);
            }
        }

        if (trailingMissing > 0) {
            var candidateCount = Math.Min(steps.Count - last - 1, 2 * trailingMissing + 2);
            var alignment = AlignBySimilarity(builder, steps, last - trailingMissing + 1, trailingMissing, last + 1, candidateCount);
            if (alignment.LastOrDefault(x => x.Op == AlignmentOp.Match) is { Op: AlignmentOp.Match } lastMatch) {
                MarkUnexpected(steps, last + 1, last + 2 + lastMatch.Actual);
            }
        }

        return steps;

        static List<AlignmentStep> AlignBySimilarity(
            LineBuilder builder,
            List<AlignmentStep> steps,
            int missingStart,
            int missingCount,
            int candidateStart,
            int candidateCount
        ) {
            return SequenceAligner.Align(
                missingCount,
                candidateCount,
                (m, c) => builder.DiffLines(steps[missingStart + m].Expected, steps[candidateStart + c].Actual).Similarity
                          >= MinSimilarityForChangedLine,
                _ => false,
                partial: true
            );
        }

        static void MarkUnexpected(List<AlignmentStep> steps, int start, int end) {
            for (var i = start; i < end; i++) {
                steps[i] = steps[i] with { Op = AlignmentOp.Unexpected };
            }
        }
    }

    /// <summary>
    /// When no line of a fragment occurs in the actual text, the alignment has nothing to anchor on. In that case,
    /// align the fragment with the region around the actual line most similar to any of its lines so that the
    /// differences are reported as changed lines instead of only "missing".
    /// </summary>
    private static List<AlignmentStep>? AlignAtMostSimilarLine(LineBuilder builder, LinePattern[] patterns, int actualCount) {
        const int maxComparisons = 100_000;
        if ((long)patterns.Length * actualCount > maxComparisons) {
            return null;
        }

        int bestExpected = -1, bestActual = -1;
        var bestSimilarity = MinSimilarityForChangedLine;
        for (var e = 0; e < patterns.Length; e++) {
            if (patterns[e].IsAnyLines) {
                continue;
            }

            for (var a = 0; a < actualCount; a++) {
                var similarity = builder.DiffLines(e, a).Similarity;
                if (similarity > bestSimilarity) {
                    (bestExpected, bestActual, bestSimilarity) = (e, a, similarity);
                }
            }
        }

        if (bestExpected < 0) {
            return null;
        }

        var start = Math.Max(0, bestActual - bestExpected);
        var length = Math.Min(actualCount - start, patterns.Length);
        var window = SequenceAligner.Align(
            patterns.Length,
            length,
            (_, _) => false,
            e => patterns[e].IsAnyLines,
            partial: false
        );

        var steps = new List<AlignmentStep>(actualCount + patterns.Length);
        for (var a = 0; a < start; a++) {
            steps.Add(new AlignmentStep(AlignmentOp.Outside, -1, a));
        }

        steps.AddRange(window.Select(x => x with { Actual = x.Actual < 0 ? -1 : x.Actual + start }));
        for (var a = start + length; a < actualCount; a++) {
            steps.Add(new AlignmentStep(AlignmentOp.Outside, -1, a));
        }

        return steps;
    }

    private sealed class LineBuilder(SourceLines expected, SourceLines actual, SourceComparisonOptions options, bool isFragment) {
        public List<SourceDiffLine> Lines { get; } = new();

        public void Add(AlignmentStep step) {
            var kind = step.Op switch {
                AlignmentOp.Match => SourceDiffLineKind.Unchanged,
                AlignmentOp.WildcardAbsorb or AlignmentOp.WildcardEmpty => SourceDiffLineKind.Placeholder,
                AlignmentOp.Outside => SourceDiffLineKind.Outside,
                _ => throw new InvalidOperationException($"Unexpected alignment step {step.Op}.")
            };

            Lines.Add(Create(kind, step.Expected, step.Actual, [], []));
        }

        /// <summary>
        /// Adds a block of missing and unexpected lines. Lines that resemble each other are paired up as changed
        /// lines so that the exact differences within them can be shown.
        /// </summary>
        public void AddDifferences(List<int> missing, List<int> unexpected) {
            var nextMissing = 0;
            var nextUnexpected = 0;
            var isOneToOne = missing.Count == 1 && unexpected.Count == 1;
            for (var m = 0; m < missing.Count && nextUnexpected < unexpected.Count; m++) {
                var bestCandidate = -1;
                TokenDiff.Result bestDiff = default;
                var lookaheadEnd = Math.Min(unexpected.Count, nextUnexpected + MaxChangedLineLookahead);
                for (var u = nextUnexpected; u < lookaheadEnd; u++) {
                    var diff = DiffLines(missing[m], unexpected[u]);
                    if ((isOneToOne || diff.Similarity >= MinSimilarityForChangedLine)
                        && (bestCandidate < 0 || diff.Similarity > bestDiff.Similarity)) {
                        bestCandidate = u;
                        bestDiff = diff;
                    }
                }

                if (bestCandidate < 0) {
                    continue;
                }

                for (; nextMissing < m; nextMissing++) {
                    Lines.Add(Create(SourceDiffLineKind.Missing, missing[nextMissing], -1, [], []));
                }

                for (; nextUnexpected < bestCandidate; nextUnexpected++) {
                    Lines.Add(Create(SourceDiffLineKind.Unexpected, -1, unexpected[nextUnexpected], [], []));
                }

                Lines.Add(Create(SourceDiffLineKind.Changed, missing[m], unexpected[bestCandidate], bestDiff.Expected, bestDiff.Actual));
                nextMissing = m + 1;
                nextUnexpected = bestCandidate + 1;
            }

            for (; nextMissing < missing.Count; nextMissing++) {
                Lines.Add(Create(SourceDiffLineKind.Missing, missing[nextMissing], -1, [], []));
            }

            for (; nextUnexpected < unexpected.Count; nextUnexpected++) {
                Lines.Add(Create(SourceDiffLineKind.Unexpected, -1, unexpected[nextUnexpected], [], []));
            }

            missing.Clear();
            unexpected.Clear();
        }

        public TokenDiff.Result DiffLines(int expectedIndex, int actualIndex) {
            return TokenDiff.Compute(
                expected.All[expected.Compared[expectedIndex]],
                actual.All[actual.Compared[actualIndex]],
                options.Whitespace,
                matchAnyPrefix: isFragment && expectedIndex == 0,
                matchAnySuffix: isFragment && expectedIndex == expected.Compared.Length - 1
            );
        }

        private SourceDiffLine Create(
            SourceDiffLineKind kind,
            int expectedIndex,
            int actualIndex,
            SourceTextRange[] expectedChanges,
            SourceTextRange[] actualChanges
        ) {
            int? expectedLine = expectedIndex >= 0 ? expected.Compared[expectedIndex] : null;
            int? actualLine = actualIndex >= 0 ? actual.Compared[actualIndex] : null;
            return new SourceDiffLine(
                kind,
                expectedLine + 1,
                expectedLine is { } e ? expected.All[e] : null,
                expectedChanges,
                actualLine + 1,
                actualLine is { } a ? actual.All[a] : null,
                actualChanges
            );
        }
    }
}

public enum SourceDiffLineKind {
    /// <summary>The expected line matches the actual line.</summary>
    Unchanged,

    /// <summary>An expected placeholder line (<c>// ...</c>) and an actual line it matched, if any.</summary>
    Placeholder,

    /// <summary>The expected line was changed into the actual line.</summary>
    Changed,

    /// <summary>The expected line does not exist in the actual text.</summary>
    Missing,

    /// <summary>The actual line was not expected.</summary>
    Unexpected,

    /// <summary>An actual line outside of the region compared against an expected fragment.</summary>
    Outside
}

/// <param name="ExpectedLineNumber">The 1-based line number in the expected text, if the line is part of it.</param>
/// <param name="ExpectedChanges">For <see cref="SourceDiffLineKind.Changed"/> lines, the parts of the expected line that differ.</param>
/// <param name="ActualLineNumber">The 1-based line number in the actual text, if the line is part of it.</param>
/// <param name="ActualChanges">For <see cref="SourceDiffLineKind.Changed"/> lines, the parts of the actual line that differ.</param>
public sealed record SourceDiffLine(
    SourceDiffLineKind Kind,
    int? ExpectedLineNumber,
    string? ExpectedText,
    IReadOnlyList<SourceTextRange> ExpectedChanges,
    int? ActualLineNumber,
    string? ActualText,
    IReadOnlyList<SourceTextRange> ActualChanges
) {
    public bool IsDifference => Kind is SourceDiffLineKind.Changed or SourceDiffLineKind.Missing or SourceDiffLineKind.Unexpected;
}

public sealed class SourceDiffResult {
    internal SourceDiffResult(
        string expected,
        string actual,
        string[] expectedLines,
        string[] actualLines,
        SourceComparisonOptions options,
        bool isFragment,
        IReadOnlyList<SourceDiffLine> lines
    ) {
        Expected = expected;
        Actual = actual;
        ExpectedLines = expectedLines;
        ActualLines = actualLines;
        Options = options;
        IsFragment = isFragment;
        Lines = lines;

        foreach (var line in lines) {
            switch (line.Kind) {
                case SourceDiffLineKind.Changed: ChangedLineCount++; break;
                case SourceDiffLineKind.Missing: MissingLineCount++; break;
                case SourceDiffLineKind.Unexpected: UnexpectedLineCount++; break;
                case SourceDiffLineKind.Unchanged: UnchangedLineCount++; break;
            }
        }
    }

    public string Expected { get; }
    public string Actual { get; }
    public SourceComparisonOptions Options { get; }

    /// <summary>Whether the expected text was compared as a fragment of the actual text.</summary>
    public bool IsFragment { get; }

    /// <summary>The aligned lines of both texts. Lines ignored by the <see cref="Options"/> are not included.</summary>
    public IReadOnlyList<SourceDiffLine> Lines { get; }

    public bool IsMatch => ChangedLineCount == 0 && MissingLineCount == 0 && UnexpectedLineCount == 0;

    public int ChangedLineCount { get; }
    public int MissingLineCount { get; }
    public int UnexpectedLineCount { get; }
    public int UnchangedLineCount { get; }

    internal string[] ExpectedLines { get; }
    internal string[] ActualLines { get; }

    /// <summary>Creates a human-readable report of the differences.</summary>
    public string Format(string? sourceName = null) {
        return SourceDiffFormatter.Format(this, sourceName);
    }

    public override string ToString() {
        return Format();
    }
}

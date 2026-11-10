using System.Text;

namespace Mumei.CodeGen.Roslyn.Testing;

internal static class SourceDiffFormatter {
    private const string Separator = " │ ";

    public static string Format(SourceDiffResult result, string? sourceName) {
        var style = new Style(result.Options.UseColors);
        var sb = new StringBuilder();

        if (result.IsMatch) {
            sb.AppendLine(result.IsFragment ? "The expected fragment was found in the source." : "The source matches the expected source.");
            return sb.ToString();
        }

        sb.AppendLine(result.IsFragment
            ? "The expected fragment was not found in the source."
            : "The source does not match the expected source.");
        sb.AppendLine();

        AppendSummary(sb, result, sourceName);
        AppendFirstDifference(sb, result, style);
        AppendHints(sb, result);
        AppendDiff(sb, result, style);
        AppendFullText(sb, result.IsFragment ? "Expected fragment" : "Expected", result.ExpectedLines);
        AppendFullText(sb, sourceName is null ? "Actual" : $"Actual: {sourceName}", result.ActualLines);

        return sb.ToString().TrimEnd();
    }

    private static void AppendSummary(StringBuilder sb, SourceDiffResult result, string? sourceName) {
        var options = result.Options;
        var comparison = result.IsFragment ? "fragment" : "full content";
        comparison += $", whitespace: {options.Whitespace}";
        if (options.IgnoreBlankLines) {
            comparison += ", blank lines ignored";
        }

        if (sourceName is not null) {
            AppendField(sb, "Source", sourceName);
        }

        AppendField(sb, "Comparison", comparison);
        AppendField(sb, "Expected", Lines(CountLines(result.ExpectedLines)));
        AppendField(sb, "Actual", Lines(CountLines(result.ActualLines)));

        var parts = new List<string>();
        if (result.ChangedLineCount > 0) {
            parts.Add($"{result.ChangedLineCount} changed");
        }

        if (result.MissingLineCount > 0) {
            parts.Add($"{result.MissingLineCount} missing");
        }

        if (result.UnexpectedLineCount > 0) {
            parts.Add($"{result.UnexpectedLineCount} unexpected");
        }

        var hunkCount = GetHunks(result).Count;
        AppendField(sb, "Differences", $"{string.Join(", ", parts)} ({Plural(hunkCount, "block")})");

        if (result.IsFragment) {
            var compared = result.Lines.Where(x => x.ExpectedLineNumber is not null && x.Kind != SourceDiffLineKind.Placeholder).ToArray();
            var actualLines = result.Lines.Where(x => x.Kind != SourceDiffLineKind.Outside && x.ActualLineNumber is not null)
                .Select(x => x.ActualLineNumber!.Value)
                .ToArray();
            var region = actualLines.Length > 0 ? $"actual {LineRange(actualLines)}, " : "";
            AppendField(sb, "Closest match", $"{region}{result.UnchangedLineCount} of {Lines(compared.Length)} matched");
        }

        sb.AppendLine();
    }

    private static void AppendFirstDifference(StringBuilder sb, SourceDiffResult result, Style style) {
        var index = result.Lines.ToList().FindIndex(x => x.IsDifference);
        var line = result.Lines[index];
        switch (line.Kind) {
            case SourceDiffLineKind.Changed: {
                var expectedChange = line.ExpectedChanges.Count > 0 ? line.ExpectedChanges[0] : (SourceTextRange?)null;
                var actualChange = line.ActualChanges.Count > 0 ? line.ActualChanges[0] : (SourceTextRange?)null;
                sb.AppendLine(
                    $"First difference at expected line {Position(line.ExpectedLineNumber!.Value, expectedChange)}, actual line {Position(line.ActualLineNumber!.Value, actualChange)}:"
                );
                sb.AppendLine($"  expected: {Snippet(line.ExpectedText!, expectedChange, style.Expected)}");
                sb.AppendLine($"  actual:   {Snippet(line.ActualText!, actualChange, style.Actual)}");
                break;
            }
            case SourceDiffLineKind.Missing: {
                var previousActual = result.Lines.Take(index).LastOrDefault(x => x.ActualLineNumber is not null)?.ActualLineNumber;
                var location = result.IsFragment && result.UnchangedLineCount == 0 ? "not found in the actual source"
                    : previousActual is null ? "missing at the start of the actual source"
                    : $"missing after actual line {previousActual}";
                sb.AppendLine($"First difference: expected line {line.ExpectedLineNumber} is {location}:");
                sb.AppendLine($"  expected: {style.Expected(Quote(line.ExpectedText!.Trim()))}");
                break;
            }
            case SourceDiffLineKind.Unexpected:
                sb.AppendLine($"First difference: actual line {line.ActualLineNumber} was not expected:");
                sb.AppendLine($"  actual:   {style.Actual(Quote(line.ActualText!.Trim()))}");
                break;
        }

        sb.AppendLine();

        static string Position(int lineNumber, SourceTextRange? change) {
            return change is { } c ? $"{lineNumber}:{c.Start + 1}" : lineNumber.ToString();
        }

        static string Snippet(string text, SourceTextRange? change, Func<string, string> color) {
            return change is { } c ? color(Quote(VisibleWhitespace(text.Substring(c.Start, c.Length)))) : "(nothing, see diff below)";
        }
    }

    private static void AppendHints(StringBuilder sb, SourceDiffResult result) {
        var hints = new List<string>();
        if (string.IsNullOrWhiteSpace(result.Actual)) {
            hints.Add("The actual source is empty.");
        }
        else if (result.IsFragment && result.UnchangedLineCount == 0) {
            hints.Add(result.ChangedLineCount > 0
                ? "None of the expected lines occur in the source; the diff shows the most similar lines."
                : "None of the expected lines occur in the source. Is this the right file?");
        }

        var lenient = result.Options with { Whitespace = WhitespaceHandling.Normalize, IgnoreBlankLines = true };
        if (result.Options != lenient) {
            var lenientResult = result.IsFragment
                ? SourceDiff.CompareFragment(result.Expected, result.Actual, lenient)
                : SourceDiff.Compare(result.Expected, result.Actual, lenient);
            if (lenientResult.IsMatch) {
                hints.Add("The texts only differ in whitespace or blank lines.");
            }
        }

        foreach (var hint in hints) {
            sb.AppendLine($"Hint: {hint}");
        }

        if (hints.Count > 0) {
            sb.AppendLine();
        }
    }

    private static void AppendDiff(StringBuilder sb, SourceDiffResult result, Style style) {
        var lines = result.Lines;
        var width = Math.Max(
            NumberWidth(result.ExpectedLines.Length),
            NumberWidth(result.ActualLines.Length)
        );

        sb.AppendLine(
            $"Diff ({style.Expected("- expected")}, {style.Actual("+ actual")}, {style.Dim("· outside of the compared region")}, ^ marks changed text):"
        );

        foreach (var (start, end) in GetHunks(result)) {
            sb.AppendLine();
            sb.AppendLine(style.Header($"@@ expected {Range(lines, start, end, x => x.ExpectedLineNumber)} · actual {Range(lines, start, end, x => x.ActualLineNumber)} @@"));
            for (var i = start; i < end; i++) {
                AppendDiffLine(sb, lines[i], width, style);
            }
        }

        sb.AppendLine();

        static string Range(IReadOnlyList<SourceDiffLine> lines, int start, int end, Func<SourceDiffLine, int?> selector) {
            return LineRange(lines.Skip(start).Take(end - start).Select(selector).OfType<int>().ToArray());
        }
    }

    private static string LineRange(int[] numbers) {
        return numbers.Length switch {
            0 => "(none)",
            _ when numbers.Min() == numbers.Max() => $"line {numbers.Min()}",
            _ => $"lines {numbers.Min()}–{numbers.Max()}"
        };
    }

    private static void AppendDiffLine(StringBuilder sb, SourceDiffLine line, int width, Style style) {
        switch (line.Kind) {
            case SourceDiffLineKind.Unchanged:
                AppendRow(' ', line.ExpectedLineNumber, line.ActualLineNumber, line.ActualText!, null);
                break;
            case SourceDiffLineKind.Placeholder:
                AppendRow(' ', line.ExpectedLineNumber, line.ActualLineNumber, line.ActualText ?? line.ExpectedText!, style.Dim);
                break;
            case SourceDiffLineKind.Outside:
                AppendRow('·', null, line.ActualLineNumber, line.ActualText!, style.Dim);
                break;
            case SourceDiffLineKind.Missing:
                AppendRow('-', line.ExpectedLineNumber, null, VisibleIfBlank(line.ExpectedText!), style.Expected);
                break;
            case SourceDiffLineKind.Unexpected:
                AppendRow('+', null, line.ActualLineNumber, VisibleIfBlank(line.ActualText!), style.Actual);
                break;
            case SourceDiffLineKind.Changed:
                AppendChangedRow('-', line.ExpectedLineNumber, null, line.ExpectedText!, line.ExpectedChanges, style.Expected);
                AppendChangedRow('+', null, line.ActualLineNumber, line.ActualText!, line.ActualChanges, style.Actual);
                break;
        }

        void AppendRow(char marker, int? expectedNumber, int? actualNumber, string text, Func<string, string>? color) {
            var row = $"{marker} {Number(expectedNumber)} {Number(actualNumber)}{Separator}{text}";
            sb.AppendLine(color is null ? row : color(row));
        }

        void AppendChangedRow(
            char marker,
            int? expectedNumber,
            int? actualNumber,
            string text,
            IReadOnlyList<SourceTextRange> changes,
            Func<string, string> color
        ) {
            var content = new StringBuilder();
            var carets = new StringBuilder();
            var position = 0;
            foreach (var change in changes) {
                var unchanged = text[position..change.Start];
                content.Append(color(unchanged));
                content.Append(style.Highlight(color, VisibleWhitespace(text.Substring(change.Start, change.Length))));
                carets.Append(Blank(unchanged)).Append('^', change.Length);
                position = change.End;
            }

            content.Append(color(text[position..]));

            var gutter = $"{Number(expectedNumber)} {Number(actualNumber)}";
            sb.AppendLine($"{color($"{marker} {gutter}{Separator}")}{content}");
            if (carets.Length > 0) {
                sb.AppendLine($"  {new string(' ', gutter.Length)}{Separator}{color(carets.ToString())}");
            }
        }

        string Number(int? number) {
            return (number?.ToString() ?? "").PadLeft(width);
        }

        static string VisibleIfBlank(string text) {
            return text.Length == 0 ? "(empty line)" : string.IsNullOrWhiteSpace(text) ? VisibleWhitespace(text) : text;
        }

        // Keeps tabs so that the carets line up with the text above when the terminal expands tabs.
        static string Blank(string text) {
            return string.Concat(text.Select(c => c == '\t' ? '\t' : ' '));
        }
    }

    private static void AppendFullText(StringBuilder sb, string title, string[] lines) {
        var count = CountLines(lines);
        var width = NumberWidth(count);
        sb.AppendLine($"{title} ({Lines(count)}):");
        for (var i = 0; i < count; i++) {
            sb.AppendLine($"{(i + 1).ToString().PadLeft(width + 2)}{Separator}{lines[i]}");
        }

        sb.AppendLine();
    }

    /// <summary>Groups differences that are at most two context-lengths apart into blocks of [start, end) indices.</summary>
    private static List<(int Start, int End)> GetHunks(SourceDiffResult result) {
        var context = Math.Max(0, result.Options.ContextLines);
        var hunks = new List<(int Start, int End)>();
        var lines = result.Lines;
        for (var i = 0; i < lines.Count; i++) {
            if (!lines[i].IsDifference) {
                continue;
            }

            var start = Math.Max(0, i - context);
            var end = Math.Min(lines.Count, i + context + 1);
            if (hunks.Count > 0 && start <= hunks[^1].End) {
                hunks[^1] = (hunks[^1].Start, end);
            }
            else {
                hunks.Add((start, end));
            }
        }

        return hunks;
    }

    private static int CountLines(string[] lines) {
        // A trailing line break does not start another line.
        return lines.Length > 1 && lines[^1].Length == 0 ? lines.Length - 1 : lines.Length;
    }

    private static int NumberWidth(int count) {
        return Math.Max(1, count.ToString().Length);
    }

    private static string Lines(int count) {
        return Plural(count, "line");
    }

    private static string Plural(int count, string noun) {
        return count == 1 ? $"1 {noun}" : $"{count} {noun}s";
    }

    private static void AppendField(StringBuilder sb, string name, string value) {
        sb.AppendLine($"  {name,-14}{value}");
    }

    private static string Quote(string text) {
        return $"`{text}`";
    }

    private static string VisibleWhitespace(string text) {
        return text.Replace(' ', '·').Replace('\t', '→');
    }

    private readonly struct Style(bool enabled) {
        public string Expected(string text) {
            return Paint(text, "32");
        }

        public string Actual(string text) {
            return Paint(text, "31");
        }

        public string Dim(string text) {
            return Paint(text, "2");
        }

        public string Header(string text) {
            return Paint(text, "36");
        }

        public string Highlight(Func<string, string> color, string text) {
            return enabled ? $"\e[7m{color(text)}\e[27m" : text;
        }

        private string Paint(string text, string code) {
            return enabled && text.Length > 0 ? $"\e[{code}m{text}\e[0m" : text;
        }
    }
}

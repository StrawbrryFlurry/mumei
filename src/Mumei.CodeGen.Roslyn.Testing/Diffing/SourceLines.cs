using System.Text;

namespace Mumei.CodeGen.Roslyn.Testing.Diffing;

/// <summary>The lines of a text along with the normalized keys of the lines that take part in a comparison.</summary>
internal sealed class SourceLines {
    private SourceLines(string[] all, int[] compared, string[] keys) {
        All = all;
        Compared = compared;
        Keys = keys;
    }

    /// <summary>All lines of the original text.</summary>
    public string[] All { get; }

    /// <summary>Indices into <see cref="All" /> of the lines that are compared.</summary>
    public int[] Compared { get; }

    /// <summary>The normalized comparison key of each compared line.</summary>
    public string[] Keys { get; }

    public static SourceLines Create(string text, SourceComparisonOptions options) {
        var all = text.ReplaceLineEndings("\n").Split('\n');

        int start = 0, end = all.Length;
        if (options.Whitespace != WhitespaceHandling.Exact) {
            while (start < end && string.IsNullOrWhiteSpace(all[start])) {
                start++;
            }

            while (end > start && string.IsNullOrWhiteSpace(all[end - 1])) {
                end--;
            }
        }

        var compared = new List<int>(end - start);
        for (var i = start; i < end; i++) {
            if (options.IgnoreBlankLines && string.IsNullOrWhiteSpace(all[i])) {
                continue;
            }

            compared.Add(i);
        }

        var keys = compared.Select(i => Normalize(all[i], options.Whitespace)).ToArray();
        return new SourceLines(all, compared.ToArray(), keys);
    }

    public static string Normalize(string line, WhitespaceHandling whitespace) {
        switch (whitespace) {
            case WhitespaceHandling.Exact:
                return line;
            case WhitespaceHandling.IgnoreIndentation:
                return line.Trim();
        }

        var trimmed = line.AsSpan().Trim();
        var sb = new StringBuilder(trimmed.Length);
        var previousWasWhitespace = false;
        foreach (var c in trimmed) {
            var isWhitespace = char.IsWhiteSpace(c);
            if (!isWhitespace || !previousWasWhitespace) {
                sb.Append(isWhitespace ? ' ' : c);
            }

            previousWasWhitespace = isWhitespace;
        }

        return sb.ToString();
    }
}

/// <summary>
///     A single expected line which may contain placeholders.
///     <list type="bullet">
///         <item><c>/*...*/</c> within a line matches any text in that line.</item>
///         <item>A line consisting only of <c>// ...</c> or <c>/*...*/</c> matches any number of lines (including none).</item>
///     </list>
/// </summary>
internal sealed class LinePattern {
    public const string AnyText = "/*...*/";
    public const string AnyLines = "// ...";
    private readonly string[]? _parts;

    private readonly string _text;

    private LinePattern(string text, string[]? parts, bool isAnyLines) {
        _text = text;
        _parts = parts;
        IsAnyLines = isAnyLines;
    }

    /// <summary>Whether this line matches any number of lines.</summary>
    public bool IsAnyLines { get; }

    public static bool IsAnyLinesPlaceholder(string line) {
        var trimmed = line.AsSpan().Trim();
        return trimmed.SequenceEqual(AnyText) || trimmed.SequenceEqual(AnyLines) || trimmed.SequenceEqual("//...");
    }

    /// <param name="line">The normalized expected line.</param>
    /// <param name="matchAnyPrefix">Whether the line may be preceded by any text (e.g. the first line of a fragment).</param>
    /// <param name="matchAnySuffix">Whether the line may be followed by any text (e.g. the last line of a fragment).</param>
    public static LinePattern Create(string line, bool matchAnyPrefix = false, bool matchAnySuffix = false) {
        if (IsAnyLinesPlaceholder(line)) {
            return new LinePattern(line, null, true);
        }

        if (!matchAnyPrefix && !matchAnySuffix && !line.Contains(AnyText, StringComparison.Ordinal)) {
            return new LinePattern(line, null, false);
        }

        // Each "/*...*/" (and an implicit prefix / suffix) splits the line into literal parts which must appear in order.
        var parts = line.Split(AnyText);
        if (matchAnyPrefix) {
            parts = ["", .. parts];
        }

        if (matchAnySuffix) {
            parts = [.. parts, ""];
        }

        return new LinePattern(line, parts, false);
    }

    public bool Matches(string text) {
        if (IsAnyLines) {
            return true;
        }

        if (_parts is null) {
            return string.Equals(_text, text, StringComparison.Ordinal);
        }

        var first = _parts[0];
        var last = _parts[^1];
        if (!text.StartsWith(first, StringComparison.Ordinal)) {
            return false;
        }

        var position = first.Length;
        for (var i = 1; i < _parts.Length - 1; i++) {
            var index = text.IndexOf(_parts[i], position, StringComparison.Ordinal);
            if (index < 0) {
                return false;
            }

            position = index + _parts[i].Length;
        }

        return text.Length - position >= last.Length && text.EndsWith(last, StringComparison.Ordinal);
    }
}
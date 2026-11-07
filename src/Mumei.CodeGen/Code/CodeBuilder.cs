using System.Text;

namespace Mumei.CodeGen;

/// <summary>
/// Builds indented lines of code, e.g. method bodies:
/// <code>
/// body.Block($"foreach (var item in {items})", loop => loop.Line($"{handle}(item);"));
/// </code>
/// </summary>
public sealed class CodeBuilder {
    private const string IndentUnit = "    ";

    private readonly StringBuilder _code = new();
    private string _indent = "";

    /// <summary>Appends one or more lines at the current indentation.</summary>
    public CodeBuilder Line(Code line) {
        foreach (var part in SplitLines(line.ToString())) {
            AppendLine(part);
        }

        return this;
    }

    public CodeBuilder Line() {
        _code.Append('\n');
        return this;
    }

    /// <summary>
    /// Appends a multi-line snippet, removing the indentation shared by all of its lines,
    /// so that raw string literals can be indented naturally.
    /// </summary>
    public CodeBuilder Lines(Code text) {
        var lines = SplitLines(text.ToString());
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0])) {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1])) {
            lines.RemoveAt(lines.Count - 1);
        }

        var commonIndent = lines.Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Length - l.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();

        foreach (var line in lines) {
            AppendLine(line.Length >= commonIndent ? line[commonIndent..] : line.TrimStart());
        }

        return this;
    }

    /// <summary>Appends <paramref name="header"/> followed by a braced, indented block.</summary>
    public CodeBuilder Block(Code header, Action<CodeBuilder> body) {
        Line($"{header} {{");
        Indent(body);
        return Line("}");
    }

    public CodeBuilder Block(Action<CodeBuilder> body) {
        Line("{");
        Indent(body);
        return Line("}");
    }

    public CodeBuilder Indent(Action<CodeBuilder> body) {
        var previous = _indent;
        _indent += IndentUnit;
        body(this);
        _indent = previous;
        return this;
    }

    public override string ToString() {
        return _code.Length > 0 && _code[^1] == '\n' ? _code.ToString(0, _code.Length - 1) : _code.ToString();
    }

    private void AppendLine(string line) {
        if (!string.IsNullOrWhiteSpace(line)) {
            _code.Append(_indent).Append(line);
        }

        _code.Append('\n');
    }

    internal static List<string> SplitLines(string text) {
        return text.Replace("\r\n", "\n").Split('\n').ToList();
    }
}

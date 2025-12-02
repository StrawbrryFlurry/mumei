using System.Runtime.CompilerServices;
using System.Text;

namespace Mumei.CodeGen.Rendering;

internal interface ISyntaxWriter {
    public void Write(in ReadOnlySpan<char> str);
    public void WriteLine(in ReadOnlySpan<char> line);
}

internal sealed class SyntaxWriter : ISyntaxWriter {
    internal const char IndentChar = ' ';
    internal const int IndentSpacing = 4;
    internal const string NewLine = "\n";

    private readonly StringBuilder _code = new();
    public int Length => _code.Length;

    private string _indentString = "";
    private bool _requiresIndent = true;

    public int IndentLevel {
        get;
        set {
            field = value > 0 ? value : 0;
            RecalculateIndent(ref _indentString, value);
        }
    }

    public void SetIndentLevel(int level) {
        IndentLevel = level;
    }

    public void WriteBlock(in ReadOnlySpan<char> block) {
        var @this = this;
        WriteBlock(ref @this, block);
    }

    public string GetIndent() {
        return _indentString;
    }

    public void Indent() {
        IndentLevel++;
    }

    public void Dedent() {
        IndentLevel--;
    }

    public void Write(string text) {
        TryWriteIndent();
        _code.Append(text);
    }

    public unsafe void Write(in ReadOnlySpan<char> text) {
        TryWriteIndent();
        if (text.Length <= 0) {
            return;
        }

        fixed (char* ptr = &text.GetPinnableReference()) {
            _code.Append(ptr, text.Length);
        }
    }

    public void Write(StringBuilder builder) {
        TryWriteIndent();
        _code.Append(builder);
    }

    public void CopyTo(Span<char> buffer) {
        _code.ToString().AsSpan().CopyTo(buffer);
    }

    public void WriteLiteral<T>(T literal) where T : notnull {
        TryWriteIndent();
        _code.Append(literal);
    }

    public void WriteLine(in ReadOnlySpan<char> line) {
        Write(line);
        _code.AppendLine();
        _requiresIndent = true;
    }

    private void TryWriteIndent() {
        if (_requiresIndent) {
            _code.Append(GetIndent());
            _requiresIndent = false;
        }
    }

    public void WriteLine() {
        _code.Append(NewLine);
        _requiresIndent = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void RecalculateIndent(ref string indentString, int indentLevel) {
        var indentationCharCount = indentLevel * IndentSpacing;
        indentString = indentationCharCount switch {
            0 => "",
            2 => "  ",
            4 => "    ",
            6 => "      ",
            8 => "        ",
            10 => "          ",
            12 => "            ",
            14 => "              ",
            16 => "                ",
            18 => "                  ",
            20 => "                    ",
            22 => "                      ",
            24 => "                        ",
            26 => "                          ",
            28 => "                            ",
            30 => "                              ",
            32 => "                                ",
            _ => new string(IndentChar, indentationCharCount)
        };
    }

    internal static void WriteBlock<TWriter>(
        ref TWriter writer,
        in ReadOnlySpan<char> block
    ) where TWriter : ISyntaxWriter {
        if (block.IsEmpty) {
            return;
        }

        var remainingLines = block;
        while (!remainingLines.IsEmpty) {
            var lineEnd = DetermineLineEnd(remainingLines, out var toSkip);
            var line = remainingLines[..lineEnd];
            var isLastLine = lineEnd + toSkip >= remainingLines.Length;
            if (isLastLine && line.Trim().IsEmpty) {
                break;
            }

            if (isLastLine) {
                writer.Write(line);
                break;
            }

            writer.WriteLine(line);
            remainingLines = remainingLines[(lineEnd + toSkip)..];
        }

        static int DetermineLineEnd(ReadOnlySpan<char> remainingLines, out int toSkip) {
            var nextNewLine = remainingLines.IndexOf('\n');
            if (nextNewLine == 0) {
                toSkip = 1;
                return 0;
            }

            if (remainingLines.IsEmpty) {
                toSkip = 0;
                return 0;
            }

            if (nextNewLine == -1) {
                toSkip = 0;
                return remainingLines.Length;
            }

            var skipCarriageReturn = remainingLines[nextNewLine - 1] is '\r';
            toSkip = skipCarriageReturn ? 2 : 1;
            return skipCarriageReturn ? nextNewLine - 1 : nextNewLine;
        }
    }

    public override string ToString() {
        return _code.ToString();
    }

    public void Clear() {
        IndentLevel = 0;
        _requiresIndent = true;
        _code.Clear();
    }
}
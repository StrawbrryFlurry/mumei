using System.Runtime.CompilerServices;
using System.Text;

namespace Mumei.CodeGen.Qt.Output;

public interface ISyntaxWriter {
    public int Length { get; }

    public void Indent();
    public void Dedent();

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="text"></param>
    public void Write(in ReadOnlySpan<char> text);

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="builder"></param>
    public void Write(StringBuilder builder);

    public void CopyTo<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter;
    public void CopyTo(Span<char> buffer);

    public void Write<TRepresentable>(in TRepresentable representable, string? format = null) where TRepresentable : ISyntaxRepresentable;

    public void WriteLiteral<T>(T literal) where T : notnull;

    /// <summary>
    ///   Writes a new line to the code buffer
    ///   adding indentation at the start. Ends
    ///   the line with a newline.
    /// </summary>
    /// <param name="line"></param>
    public void WriteLine(in ReadOnlySpan<char> line);

    public void WriteLine();

    public void WriteBlock(in ReadOnlySpan<char> block);

    /// <summary>
    ///   Writes an empty line to the stream
    /// </summary>
    /// <returns></returns>
    public void WriteFormatted(in FormattableSyntaxWritable writable);

    public void WriteFormattedLine(in FormattableSyntaxWritable writable);
    public void WriteFormattedBlock(in FormattableSyntaxWritable writable);

    public string ToSyntax();
}

public class SyntaxWriter : ISyntaxWriter {
    public int Length => _code.Length;

    internal const char IndentChar = ' ';
    internal const int IndentSpacing = 4;

    private readonly StringBuilder _code = new();

    private string _indentString = "";
    internal const string NewLine = "\n";

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

    public void WriteFormatted(in FormattableSyntaxWritable writable) {
        TryWriteIndent();
        var @this = this;
        writable.CopyToAndDispose(ref @this);
    }

    public void WriteFormattedLine(in FormattableSyntaxWritable writable) {
        TryWriteIndent();
        var @this = this;
        writable.CopyToAndDispose(ref @this);
        WriteLine();
    }

    public void WriteFormattedBlock(in FormattableSyntaxWritable writable) {
        var tempBuffer = new ValueSyntaxWriter(stackalloc char[ValueSyntaxWriter.StackBufferSize]);
        writable.CopyToAndDispose(ref tempBuffer);
        var @this = this;
        WriteBlock(ref @this, tempBuffer.Buffer);
    }

    public string GetIndent() {
        return _indentString;
    }

    public virtual string ToSyntax() {
        return _code.ToString();
    }

    public void Indent() {
        IndentLevel++;
    }

    public void Dedent() {
        IndentLevel--;
    }

    public ISyntaxWriter Write(string text) {
        TryWriteIndent();
        _code.Append(text);
        return this;
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

    public void CopyTo<TSyntaxWriter>(ref TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(_code.ToString());
    }

    public void CopyTo(Span<char> buffer) {
        _code.ToString().AsSpan().CopyTo(buffer);
    }

    public void WriteFrom<TSyntaxWriter>(in TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        TryWriteIndent();
        if (writer is SyntaxWriter thisImpl) {
            _code.Append(thisImpl._code);
            return;
        }

        _code.Append(writer.ToSyntax());
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

    public void Write<TRepresentable>(in TRepresentable representable, string? format = null) where TRepresentable : ISyntaxRepresentable {
        TryWriteIndent();
        var @this = this;
        representable.WriteSyntax(ref @this, format);
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
        return ToSyntax();
    }
}
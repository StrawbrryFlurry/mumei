using System.Runtime.CompilerServices;
using System.Text;
using Mumei.CodeGen.Playground;

namespace Mumei.CodeGen.Qt.Output;

[InterpolatedStringHandler]
public readonly ref struct FormattableSyntaxWritable {
    private readonly SyntaxWriter _writer;

    public FormattableSyntaxWritable(int literalLength, int formattedCount, [CallerMemberName] string memberName = "") {
        _writer = new SyntaxWriter();
    }

    public void AppendLiteral(string s) {
        _writer.Write(s);
    }

    public void AppendFormatted(string s) {
        _writer.Write(s);
    }

    public void AppendFormatted<TRepresentable>(TRepresentable representable) where TRepresentable : ISyntaxRepresentable {
        representable.WriteSyntax(_writer);
    }

    public void AppendFormatted<TRepresentable>(TRepresentable representable, string format) where TRepresentable : ISyntaxRepresentable {
        representable.WriteSyntax(_writer, format);
    }

    public void AppendFormatted(Type type, string? format = null) {
        RuntimeTypeSerializer.SerializeInto(_writer, type, format);
    }

    public void AppendFormatted(AccessModifier modifier) {
        _writer.Write(modifier.AsCSharpString());
    }

    public void AppendFormatted(ParameterModifier parameterModifier) {
        if (parameterModifier == ParameterModifier.None) {
            return;
        }

        if (parameterModifier.HasFlag(ParameterModifier.This)) {
            _writer.Write("this ");
        }

        _writer.Write(parameterModifier switch {
            ParameterModifier.In => "in ",
            ParameterModifier.Out => "out ",
            ParameterModifier.Ref => "ref ",
            _ => ""
        });
    }

    public void WriteInto<TWriter>(TWriter writer) where TWriter : ISyntaxWriter {
        writer.Write(_writer);
    }
}

public interface ISyntaxWriter {
    public void Indent();
    public void Dedent();

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="text"></param>
    public void Write(ReadOnlySpan<char> text);

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="builder"></param>
    public void Write(StringBuilder builder);

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="writer"></param>
    public void Write<TSyntaxWriter>(TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter;

    /// <summary>
    ///   Writes a new line to the code buffer
    ///   adding indentation at the start. Ends
    ///   the line with a newline.
    /// </summary>
    /// <param name="line"></param>
    public void WriteLine(string line);

    public void WriteLine();

    /// <summary>
    ///   Writes an empty line to the stream
    /// </summary>
    /// <returns></returns>
    public void WriteFormatted(FormattableSyntaxWritable writable);

    public string ToSyntax();
}

public class SyntaxWriter : ISyntaxWriter {
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

    public void WriteFormatted(FormattableSyntaxWritable writable) {
        TryWriteIndent();
        writable.WriteInto(this);
    }

    public void WriteFormattedLine(FormattableSyntaxWritable writable) {
        TryWriteIndent();
        writable.WriteInto(this);
        WriteLine();
    }

    public void WriteFormattedBlock(FormattableSyntaxWritable writable) {
        TryWriteIndent();
        writable.WriteInto(this);
        WriteLine();
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

    public unsafe void Write(ReadOnlySpan<char> text) {
        fixed (char* p = text) {
            _code.Append(p, text.Length);
        }
    }

    public void Write(StringBuilder builder) {
        TryWriteIndent();
        _code.Append(builder);
    }

    public void Write<TSyntaxWriter>(TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        TryWriteIndent();
        if (writer is SyntaxWriter thisImpl) {
            _code.Append(thisImpl._code);
            return;
        }

        _code.Append(writer.ToSyntax());
    }

    public void WriteLine(string line) {
        TryWriteIndent();
        _code.AppendLine(line);
        _requiresIndent = true;
    }

    private void TryWriteIndent() {
        if (_requiresIndent) {
            _code.Append(GetIndent());
            _requiresIndent = false;
        }
    }

    public void WriteSyntax<TRepresentable>(TRepresentable representable) where TRepresentable : ISyntaxRepresentable {
        TryWriteIndent();
        representable.WriteSyntax(this);
    }

    public void WriteLine() {
        TryWriteIndent();
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

    public override string ToString() {
        return ToSyntax();
    }
}
using System.Collections.Immutable;
using System.Diagnostics.SymbolStore;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    public int IndentLevel { get; set; }

    public void Indent();
    public void UnIndent();

    public void SetIndentLevel(int level);

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="text"></param>
    public ISyntaxWriter Write(string text);

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="text"></param>
    public ISyntaxWriter Write(ReadOnlySpan<char> text);

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="builder"></param>
    public ISyntaxWriter Write(StringBuilder builder);

    /// <summary>
    ///   Appends text to the current line.
    /// </summary>
    /// <param name="writer"></param>
    public ISyntaxWriter Write<TSyntaxWriter>(TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter;

    /// <summary>
    ///   Writes a new line to the code buffer
    ///   adding indentation at the start. Ends
    ///   the line with a newline.
    /// </summary>
    /// <param name="line"></param>
    public ISyntaxWriter WriteLine(string line);

    public ISyntaxWriter WriteLine();

    /// <summary>
    ///   Writes an empty line to the stream
    /// </summary>
    /// <returns></returns>
    public ISyntaxWriter WriteFormatted(FormattableSyntaxWritable writable);

    public string GetIndent();
    public string ToSyntax();
}

public class SyntaxWriter : ISyntaxWriter {
    internal const char IndentChar = ' ';
    internal const int IndentSpacing = 4;

    private readonly StringBuilder _code = new();

    private string _indentString = "";
    internal string NewLine = Environment.NewLine;

    private bool _requiresIndent = true;

    public int IndentLevel {
        get;
        set {
            field = value > 0 ? value : 0;
            RecalculateIndent();
        }
    }

    public void SetIndentLevel(int level) {
        IndentLevel = level;
    }

    public ISyntaxWriter WriteFormatted(FormattableSyntaxWritable writable) {
        TryWriteIndent();
        writable.WriteInto(this);
        return this;
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

    public void UnIndent() {
        IndentLevel--;
    }

    public ISyntaxWriter Write(string text) {
        TryWriteIndent();
        _code.Append(text);
        return this;
    }

    public unsafe ISyntaxWriter Write(ReadOnlySpan<char> text) {
        fixed (char* p = text) {
            _code.Append(p, text.Length);
        }

        return this;
    }

    public ISyntaxWriter Write(StringBuilder builder) {
        TryWriteIndent();
        _code.Append(builder);
        return this;
    }

    public ISyntaxWriter Write<TSyntaxWriter>(TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter {
        TryWriteIndent();
        if (writer is SyntaxWriter thisImpl) {
            _code.Append(thisImpl._code);
            return this;
        }

        _code.Append(writer.ToSyntax());
        return this;
    }

    public ISyntaxWriter WriteLine(string line) {
        TryWriteIndent();
        _code.AppendLine(line);
        _requiresIndent = true;
        return this;
    }

    private void TryWriteIndent() {
        if (_requiresIndent) {
            _code.Append(GetIndent());
            _requiresIndent = false;
        }
    }

    public ISyntaxWriter WriteSyntax<TRepresentable>(TRepresentable representable) where TRepresentable : ISyntaxRepresentable {
        TryWriteIndent();
        representable.WriteSyntax(this);
        return this;
    }

    public ISyntaxWriter WriteLine() {
        TryWriteIndent();
        _code.Append(NewLine);
        _requiresIndent = true;
        return this;
    }

    private void RecalculateIndent() {
        var indentationCharCount = IndentLevel * IndentSpacing;
        _indentString = indentationCharCount switch {
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
}
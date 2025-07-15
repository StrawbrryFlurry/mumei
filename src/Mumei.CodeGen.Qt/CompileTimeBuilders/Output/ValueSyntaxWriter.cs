using System.Text;

namespace Mumei.CodeGen.Qt.Output;

internal struct ValueSyntaxWriter() : ISyntaxWriter {
    internal const char IndentChar = SyntaxWriter.IndentChar;
    internal const int IndentSpacing = SyntaxWriter.IndentSpacing;
    internal const string NewLine = SyntaxWriter.NewLine;

    private string _indentString = "";
    private int _indentLevel = 0;

    public void Indent() {
        _indentLevel++;
        SyntaxWriter.RecalculateIndent(ref _indentString, _indentLevel);
    }

    public void Dedent() {
        _indentLevel++;
        SyntaxWriter.RecalculateIndent(ref _indentString, _indentLevel);
    }

    public void Write(ReadOnlySpan<char> text) { }

    public void Write(StringBuilder builder) { }

    public void Write<TSyntaxWriter>(TSyntaxWriter writer) where TSyntaxWriter : ISyntaxWriter { }

    public void WriteLine(string line) { }

    public void WriteLine() { }

    public void WriteFormatted(FormattableSyntaxWritable writable) { }

    public string ToSyntax() {
        return "";
    }
}
using System.Text;

namespace Mumei.CodeGen.SyntaxWriters;

public class SyntaxWriter : ISyntaxWriter {
  internal const char IndentChar = ' ';
  internal const int IndentSpacing = 2;

  private readonly StringBuilder _code = new();
  private int _indentLevelLevel;

  private string _indentString = "";
  internal string NewLine = Environment.NewLine;

  public int IndentLevelLevel {
    get => _indentLevelLevel;
    set {
      _indentLevelLevel = value > 0 ? value : 0;
      RecalculateIndent();
    }
  }

  public void Indent() {
    IndentLevelLevel++;
  }

  public void UnIndent() {
    IndentLevelLevel--;
  }

  public void SetIndentLevel(int level) {
    IndentLevelLevel = level;
  }

  public void WriteLineStart(string text) {
    _code.Append(GetIndent());
    _code.Append(text);
  }


  public void WriteLineEnd(string line) {
    _code.AppendLine(line);
  }

  public void Write(string text) {
    _code.Append(text);
  }

  public void Write(Syntax.Syntax syntax) {
    Write(syntax.GetIdentifier());
  }

  public void WriteLine(string line) {
    _code.Append(GetIndent());
    _code.AppendLine(line);
  }

  public string GetIndent() {
    return _indentString;
  }

  public virtual string ToSyntax() {
    return _code.ToString();
  }

  private void RecalculateIndent() {
    var indentationCharCount = IndentLevelLevel * IndentSpacing;
    _indentString = new string(IndentChar, indentationCharCount);
  }
}
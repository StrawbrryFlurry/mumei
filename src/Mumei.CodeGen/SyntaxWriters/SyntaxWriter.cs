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

  public void SetIndentLevel(int level) {
    IndentLevelLevel = level;
  }

  public string GetIndent() {
    return _indentString;
  }

  public virtual string ToSyntax() {
    return _code.ToString();
  }

  public void Indent() {
    IndentLevelLevel++;
  }

  public void UnIndent() {
    IndentLevelLevel--;
  }

  public ISyntaxWriter WriteLineStart(string text) {
    _code.Append(GetIndent());
    _code.Append(text);
    return this;
  }

  public ISyntaxWriter WriteLineEnd(string line) {
    _code.AppendLine(line);
    return this;
  }

  public ISyntaxWriter Write(string text) {
    _code.Append(text);
    return this;
  }

  public ISyntaxWriter Write(SyntaxVisibility visibility) {
    if (visibility == SyntaxVisibility.None) {
      return this;
    }

    Write(visibility.ToVisibilityString());
    Write(" ");

    return this;
  }

  public ISyntaxWriter WriteLine(string line) {
    _code.Append(GetIndent());
    _code.AppendLine(line);

    return this;
  }

  public ISyntaxWriter WriteLine() {
    _code.Append(NewLine);
    return this;
  }

  public void Dispose() { }

  private void RecalculateIndent() {
    var indentationCharCount = IndentLevelLevel * IndentSpacing;
    _indentString = new string(IndentChar, indentationCharCount);
  }
}
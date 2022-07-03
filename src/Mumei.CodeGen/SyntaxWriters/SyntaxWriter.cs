using System.Text;

namespace Mumei.CodeGen.SyntaxWriters;

public class SyntaxWriter {
  internal const char IndentChar = ' ';
  internal const int IndentSpacing = 2;

  private readonly StringBuilder _code = new();
  private int _indentLevelLevel;

  private string _indentString = "";

  protected internal int IndentLevelLevel {
    get => _indentLevelLevel;
    set {
      _indentLevelLevel = value > 0 ? value : 0;
      RecalculateIndent();
    }
  }

  public SyntaxWriter() {
  }

  protected SyntaxWriter(int indentLevel) {
    IndentLevelLevel = indentLevel;
  }

  private void RecalculateIndent() {
    var indentationCharCount = IndentLevelLevel * IndentSpacing;
    _indentString = new string(IndentChar, indentationCharCount);
  }

  protected internal void Indent() {
    IndentLevelLevel++;
  }

  protected internal void UnIndent() {
    IndentLevelLevel--;
  }

  protected internal void SetIndentLevel(int level) {
    IndentLevelLevel = level;
  }

  /// <summary>
  ///   Writes the specified text to the line
  ///   adding the indentation before the text.
  /// </summary>
  /// <param name="text"></param>
  protected internal void WriteLineStart(string text) {
    _code.Append(GetIndent());
    _code.Append(text);
  }

  /// <summary>
  ///   Appends text to the current line.
  ///   Adds a newline after the text.
  /// </summary>
  /// <param name="line"></param>
  protected internal void WriteLineEnd(string line) {
    _code.AppendLine(line);
  }

  /// <summary>
  ///   Appends text to the current line.
  /// </summary>
  /// <param name="text"></param>
  protected internal void Write(string text) {
    _code.Append(text);
  }

  /// <summary>
  ///   Writes a new line to the code buffer
  ///   adding indentation at the start. Ends
  ///   the line with a newline.
  /// </summary>
  /// <param name="line"></param>
  protected internal void WriteLine(string line) {
    _code.Append(GetIndent());
    _code.AppendLine(line);
  }

  protected internal string GetIndent() {
    return _indentString;
  }

  public virtual string ToSyntax() {
    return _code.ToString();
  }
}
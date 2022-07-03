using System.Text;

namespace Mumei.CodeGen.SyntaxWriters;

public class SyntaxWriter {
  internal const char IndentChar = ' ';
  internal const int IndentSpacing = 2;

  private readonly StringBuilder _code = new();
  private int _indentLevel;

  private string _indentString = "";

  protected internal int IndentLevel {
    get => _indentLevel;
    set {
      _indentLevel = value > 0 ? value : 0;
      RecalculateIndent();
    }
  }

  private void RecalculateIndent() {
    var indentationCharCount = IndentLevel * IndentSpacing;
    _indentString = new string(IndentChar, indentationCharCount);
  }

  protected internal void Indent() {
    IndentLevel++;
  }

  protected internal void UnIndent() {
    IndentLevel--;
  }

  protected internal void SetIndentLevel(int level) {
    IndentLevel = level;
  }

  /// <summary>
  ///   Writes the specified text to the code buffer
  ///   adding the indentation before the text.
  /// </summary>
  /// <param name="text"></param>
  protected internal void WriteWithIndent(string text) {
    _code.Append(GetIndent());
    _code.Append(text);
  }

  /// <summary>
  ///   Writes the specified text to the code buffer
  ///   without any indentation. Adds a newline after the text.
  /// </summary>
  /// <param name="line"></param>
  protected internal void WriteNewLine(string line) {
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
  ///   Adds a new line to the code buffer adding the
  ///   current indentation at the beginning.
  /// </summary>
  /// <param name="line"></param>
  protected internal void WriteLine(string line) {
    _code.Append(GetIndent());
    _code.AppendLine(line);
  }

  protected internal string GetIndent() {
    return _indentString;
  }

  public override string ToString() {
    return _code.ToString();
  }
}
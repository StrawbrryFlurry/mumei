using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.CodeGen.SyntaxWriters;

public interface ISyntaxWriter {
  public int IndentLevelLevel { get; set; }

  public void Indent();
  public void UnIndent();

  public void SetIndentLevel(int level);

  /// <summary>
  ///   Writes the specified text to the line
  ///   adding the indentation before the text.
  /// </summary>
  /// <param name="text"></param>
  public ISyntaxWriter WriteLineStart(string text);

  /// <summary>
  ///   Appends text to the current line.
  ///   Adds a newline after the text.
  /// </summary>
  /// <param name="line"></param>
  public ISyntaxWriter WriteLineEnd(string line);

  /// <summary>
  ///   Appends text to the current line.
  /// </summary>
  /// <param name="text"></param>
  public ISyntaxWriter Write(string text);

  /// <summary>
  ///   Appends the syntax to the current line
  /// </summary>
  /// <param name="syntax"></param>
  public ISyntaxWriter Write(Syntax syntax);

  /// <summary>
  ///   Appends the string value of the
  ///   visibility with a space at the end
  ///   to the current line.
  /// </summary>
  /// <param name="visibility"></param>
  public ISyntaxWriter Write(SyntaxVisibility visibility);

  /// <summary>
  ///   Writes a new line to the code buffer
  ///   adding indentation at the start. Ends
  ///   the line with a newline.
  /// </summary>
  /// <param name="line"></param>
  public ISyntaxWriter WriteLine(string line);

  public string GetIndent();
  public string ToSyntax();
}
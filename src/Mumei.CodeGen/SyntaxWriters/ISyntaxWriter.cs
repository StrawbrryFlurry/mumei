namespace Mumei.CodeGen.SyntaxWriters;

public interface ISyntaxWriter : IDisposable {
  public int IndentLevel { get; set; }

  public void Indent();
  public void UnIndent();

  public void SetIndentLevel(int level);

  /// <summary>
  ///   Writes the specified text to the line
  ///   adding the indentation before the text.
  /// </summary>
  /// <param name="text"></param>
  public ISyntaxWriter WriteLineStart(string text);

  /// <inheritdoc cref="WriteLineStart(string)" />
  public ISyntaxWriter WriteLineStart();

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

  /// <summary>
  ///   Writes an empty line to the stream
  /// </summary>
  /// <returns></returns>
  public ISyntaxWriter WriteLine();

  public string GetIndent();
  public string ToSyntax();
}
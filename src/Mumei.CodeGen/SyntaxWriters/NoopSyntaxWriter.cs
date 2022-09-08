namespace Mumei.CodeGen.SyntaxWriters;

public class NoopSyntaxWriter : ITypeAwareSyntaxWriter {
  public static ITypeAwareSyntaxWriter Instance { get; } = new NoopSyntaxWriter();

  public void Dispose() { }

  public int IndentLevel { get; set; }

  public void Indent() { }

  public void UnIndent() { }

  public void SetIndentLevel(int level) { }

  public ISyntaxWriter WriteLineStart(string text) {
    return this;
  }

  public ISyntaxWriter WriteLineStart() {
    return this;
  }

  public ISyntaxWriter WriteLineEnd(string line) {
    return this;
  }

  public ISyntaxWriter Write(string text) {
    return this;
  }

  public ISyntaxWriter Write(SyntaxVisibility visibility) {
    return this;
  }

  public ISyntaxWriter WriteLine(string line) {
    return this;
  }

  public ISyntaxWriter WriteLine() {
    return this;
  }

  public string GetIndent() {
    return "";
  }

  public string ToSyntax() {
    return "";
  }

  public SyntaxTypeContext TypeContext { get; } = new();

  public void WriteValueAsExpressionSyntax(object value) { }

  public void WriteTypeName(Type type) { }
}
namespace Mumei.CodeGen.SyntaxWriters;

public class MemberSyntaxWriter : TypeAwareSyntaxWriter {
  /// <summary>
  ///   The base indentation level of this member
  ///   <code>
  ///   public class MyClass { -- Base indent level is 0
  ///     public void MyMethod() { -- Base indent level is 1
  ///   }
  ///   </code>
  /// </summary>
  protected int BaseIndentLevel;

  public MemberSyntaxWriter(int baseIndentLevel, WriterTypeContext ctx) : base(baseIndentLevel, ctx) {
    BaseIndentLevel = baseIndentLevel;
  }

  protected internal void WriteBlock(Action block) {
    WriteLineEnd("{");
    Indent();
    block();
    UnIndent();
    Write("}");
  }
}
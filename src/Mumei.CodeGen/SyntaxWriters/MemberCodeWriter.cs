namespace Mumei.CodeGen.SyntaxWriters;

public class MemberSyntaxWriter : SyntaxWriter {
  protected internal void WriteBlock(Action block) {
    WriteNewLine("{");
    Indent();
    block();
    UnIndent();
    Write("}");
  }
}
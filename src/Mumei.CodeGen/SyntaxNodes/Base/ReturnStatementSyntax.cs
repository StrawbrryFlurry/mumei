using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class ReturnStatementSyntax : StatementSyntax {
  public ReturnStatementSyntax(ExpressionSyntax? value, Syntax? parent = null) : base(parent) {
    value?.SetParent(this);
    Value = value;
  }

  public ReturnStatementSyntax(Syntax? parent = null) : base(parent) { }

  public ExpressionSyntax? Value { get; }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.Write("return");

    if (Value is not null) {
      writer.Write(" ");
      Value.WriteAsSyntax(writer);
    }

    writer.Write(";");
  }

  public override Syntax Clone() {
    return new ReturnStatementSyntax(Value?.Clone<ExpressionSyntax>());
  }
}
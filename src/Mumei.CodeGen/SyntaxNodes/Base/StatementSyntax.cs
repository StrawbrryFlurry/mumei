using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class StatementSyntax : Syntax {
  public StatementSyntax(Syntax? parent) : base(parent) { }
}

public class ExpressionStatementSyntax : StatementSyntax {
  private readonly ExpressionSyntax _expression;

  public ExpressionStatementSyntax(ExpressionSyntax expression, Syntax? parent = null) : base(parent) {
    _expression = expression;
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    _expression.WriteAsSyntax(writer);
    writer.Write(";");
  }

  public override Syntax Clone() {
    return new ExpressionStatementSyntax(_expression.Clone<ExpressionSyntax>());
  }
}
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

public class IfStatementSyntax : StatementSyntax {
  public readonly BlockSyntax Body;
  public readonly ExpressionSyntax Condition;

  public IfStatementSyntax(ExpressionSyntax condition, BlockSyntax body, Syntax? parent = null) : base(parent) {
    Condition = condition;
    Condition.SetParent(this);
    Body = body;
    Body.SetParent(this);
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }

  public IfStatementSyntax AddElseIfClause(ExpressionSyntax condition, BlockBuilder body) {
    throw new NotImplementedException();
  }

  public IfStatementSyntax DefineElseClause(BlockBuilder body) {
    throw new NotImplementedException();
  }
}
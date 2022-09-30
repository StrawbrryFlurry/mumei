using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class IfStatementSyntax : StatementSyntax {
  private readonly List<(ExpressionSyntax Condition, BlockSyntax Body)> _clauses = new();

  public IfStatementSyntax(Expression<Func<bool>> condition, BlockBuilder body, Syntax? parent = null)
    : this(new ExpressionSyntax(condition), body.Build(), parent) { }


  public IfStatementSyntax(ExpressionSyntax condition, BlockBuilder body, Syntax? parent = null)
    : this(condition, body.Build(), parent) { }

  public IfStatementSyntax(ExpressionSyntax condition, BlockSyntax body, Syntax? parent = null) : base(parent) {
    AddElseIf(condition, body);
  }

  private IfStatementSyntax() : base(null) { }

  public IEnumerable<(ExpressionSyntax, BlockSyntax)> Clauses => _clauses;
  public BlockSyntax? ElseClause { get; private set; }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    var ifClause = _clauses[0];

    writer.Write("if(");
    ifClause.Condition.WriteAsSyntax(writer);
    writer.Write(") ");
    ifClause.Body.WriteAsSyntax(writer);

    foreach (var elseIfClause in _clauses.Skip(1)) {
      writer.Write(" else if(");
      elseIfClause.Condition.WriteAsSyntax(writer);
      writer.Write(") ");
      elseIfClause.Body.WriteAsSyntax(writer);
    }

    if (ElseClause == null) {
      return;
    }

    writer.Write(" else ");
    ElseClause.WriteAsSyntax(writer);
  }

  public override Syntax Clone() {
    var clone = new IfStatementSyntax();

    foreach (var (condition, body) in _clauses) {
      var clonedCondition = condition.Clone<ExpressionSyntax>();
      clonedCondition.SetParent(clone);

      var clonedBody = body.Clone<BlockSyntax>();
      clonedBody.SetParent(clone);

      clone._clauses.Add((clonedCondition, clonedBody));
    }

    clone.ElseClause = ElseClause?.Clone() as BlockSyntax;

    return clone;
  }

  public void AddElseIf(Expression<Func<bool>> condition, BlockBuilder body) {
    AddElseIf(new ExpressionSyntax(condition), body.Build());
  }

  public void AddElseIf(ExpressionSyntax condition, BlockBuilder body) {
    AddElseIf(condition, body.Build());
  }

  public void AddElseIf(ExpressionSyntax condition, BlockSyntax body) {
    condition.SetParent(this);
    body.SetParent(this);

    _clauses.Add((condition, body));
  }

  public void DefineElse(BlockBuilder body) {
    DefineElse(body.Build());
  }

  public void DefineElse(BlockSyntax body) {
    body.SetParent(this);
    ElseClause = body;
  }
}
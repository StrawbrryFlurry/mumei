using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   Represents a expression through a LINQ expression
/// </summary>
public class ExpressionSyntax : Syntax {
  private readonly Expression _expression;

  public ExpressionSyntax(Expression expression, Syntax? parent = null) : base(parent) {
    _expression = expression;
  }

  public static implicit operator ExpressionSyntax(Expression expression) {
    return new ExpressionSyntax(expression);
  }

  public static implicit operator Expression(ExpressionSyntax expression) {
    return expression._expression;
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.Write(_expression.ToString());
  }

  public override string ToString() {
    return _expression.ToString();
  }
}
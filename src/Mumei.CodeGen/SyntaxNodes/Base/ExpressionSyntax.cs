using System.Linq.Expressions;
using System.Reflection;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public interface ITransformMemberExpression {
  public Expression TransformMemberAccess(Expression target, MemberInfo member);
}

public class ExpressionSyntax : Syntax {
  private static readonly SyntaxExpressionVisitor SyntaxExpressionVisitor = new();
  protected readonly Expression ExpressionNode;

  public ExpressionSyntax(Expression expressionNode, Syntax? parent = null) : base(parent) {
    if (expressionNode is not LambdaExpression lambda) {
      ExpressionNode = expressionNode;
      return;
    }

    ExpressionNode = lambda.Body;
  }

  public static implicit operator ExpressionSyntax(Expression expression) {
    return new ExpressionSyntax(expression);
  }

  public static implicit operator Expression(ExpressionSyntax expression) {
    return expression.ExpressionNode;
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.Write(ParseExpressionToSyntaxString());
  }

  public override Syntax Clone() {
    return new ExpressionSyntax(ExpressionNode);
  }

  protected internal virtual string ParseExpressionToSyntaxString() {
    return TransformInternalExpressionSyntax().ToString();
  }

  /// <summary>
  ///   Some expressions contain internal members that should not be used
  ///   in the syntax tree. The VariableExpression for example is used like
  ///   this:
  ///   <code>
  /// var v = new VariableExpression("v");
  /// var ifStmt = new IfStatement(() => v.Value == 1);
  /// </code>
  ///   The member access should not be part of the syntax tree as no member
  ///   of the variable has been accessed. This method re-maps the expression
  ///   to a single VariableExpression.
  /// </summary>
  /// <returns></returns>
  private Expression TransformInternalExpressionSyntax() {
    return SyntaxExpressionVisitor.Visit(ExpressionNode) ?? ExpressionNode;
  }

  public override string ToString() {
    return ParseExpressionToSyntaxString();
  }
}
using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class ExpressionSyntax : Syntax {
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
    writer.Write(ParseExpressionToSyntaxString(writer));
  }

  public override Syntax Clone() {
    return new ExpressionSyntax(ExpressionNode);
  }

  protected internal string ParseExpressionToSyntaxString(ITypeAwareSyntaxWriter writer) {
    var expressionString = ConvertExpressionToString(TransformInternalExpressionSyntax(writer.TypeContext));
    return RemoveSurroundingParentheses(expressionString);
  }

  private string ConvertExpressionToString(Expression expression) {
    return expression.ToString();
  }

  /// <summary>
  ///   Standalone expressions like "a + b" are wrapped in parentheses
  ///   by the LINQ Expression API. We don't need these parentheses
  ///   so we remove them.
  /// </summary>
  /// <param name="expression"></param>
  /// <returns></returns>
  private string RemoveSurroundingParentheses(string expression) {
    if (expression.StartsWith("(") && expression.EndsWith(")")) {
      return expression.Substring(1, expression.Length - 2);
    }

    return expression;
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
  private Expression TransformInternalExpressionSyntax(SyntaxTypeContext ctx) {
    var visitor = new SyntaxExpressionVisitor(ctx);
    return visitor.Visit(ExpressionNode) ?? ExpressionNode;
  }

  public override string ToString() {
    return ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance);
  }
}
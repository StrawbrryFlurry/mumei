using System.Linq.Expressions;

namespace Mumei.CodeGen.Expressions;

public class InvokeInvokableExpression : Expression {
  public InvokeInvokableExpression(string identifier, Expression[] arguments, Type returnType) {
    Identifier = identifier;
    Arguments = arguments;
    Type = returnType;
  }

  public string Identifier { get; }
  public Expression[] Arguments { get; }

  public override ExpressionType NodeType { get; } = ExpressionType.Invoke;

  /// <summary>
  ///   The return type of the method call.
  /// </summary>
  public override Type Type { get; } // The property is called type to work with other sub classes of Expression.

  public override string ToString() {
    return $"{Identifier}({GetArgumentString()})";
  }

  private string GetArgumentString() {
    return string.Join(", ", Arguments.Select(a => a.ToString()));
  }
}
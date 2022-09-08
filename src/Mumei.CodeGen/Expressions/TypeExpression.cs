using System.Linq.Expressions;

namespace Mumei.CodeGen.Expressions;

/// <summary>
///   Represents a type expression:
///   <code>
/// typeof(string)
/// </code>
///   which is usually is mal-formatted when
///   it's part of a constant expression.
/// </summary>
public class TypeExpression : Expression {
  public readonly Type ExpressionType;

  public TypeExpression(ConstantExpression type) {
    ExpressionType = (Type)type.Value;
  }

  public TypeExpression(Type type) {
    ExpressionType = type;
  }

  public override Type Type { get; } = typeof(Type);

  public override ExpressionType NodeType { get; } = System.Linq.Expressions.ExpressionType.Constant;

  public override string ToString() {
    return $"typeof({ExpressionType.Name})";
  }
}
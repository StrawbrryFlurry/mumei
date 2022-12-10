using System.Linq.Expressions;

namespace Mumei.CodeGen.Expressions;

/// <summary>
///   Replaces a <see cref="ConstantExpression" /> whose ToString
///   method returns True and False as capitalized strings.
/// </summary>
public sealed class BooleanExpression : Expression {
  public readonly bool Value;

  public BooleanExpression(bool value) {
    Value = value;
  }

  public override Type Type => typeof(bool);

  public override ExpressionType NodeType { get; } = ExpressionType.Constant;

  public override string ToString() {
    return Value ? "true" : "false";
  }
}
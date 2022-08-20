using System.Linq.Expressions;
using System.Reflection;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class ExpressionSyntaxTests {
  [Fact]
  public void ToString_ReturnsExpressionAsString() {
    var sut = new ExpressionSyntax(Expression.UnaryPlus(Expression.Constant(1)));

    sut.ToString().Should().Be("+1");
  }

  [Fact]
  public void
    ParseExpressionToSyntaxString_ReplacesTargetCallWithSpecifiedValue_WhenTargetImplementsITransformMember() {
    var closureVariable = new ImplementsITransformMemberExpression();
    var sut = new ExpressionSyntax(() => closureVariable.BoundValue == "Bar");

    sut.ParseExpressionToSyntaxString().Should().Be("(Bar == \"Bar\")");
  }

  [Fact]
  public void ParseExpressionToSyntaxString_KeepsMemberExpressionAsIs_WhenTargetDoesNotImplementITransformMember() {
    var closureVariable = Expression.Variable(typeof(string), "");
    var sut = new ExpressionSyntax(() => closureVariable.Name == "Bar");

    sut.ParseExpressionToSyntaxString().Should().Be("(Bar == \"Bar\")");
  }

  [Fact]
  public void ShouldTransformExpressionMember_ReturnsTrue_WhenTypeInheritsFromITransformableExpression() {
    ExpressionSyntaxVisitor
      .ShouldTransformExpressionMember(new ImplementsITransformMemberExpression())
      .Should()
      .BeTrue();
  }

  [Fact]
  public void ShouldTransformExpressionMember_ReturnsFalse_WhenTypeDoesNotInheritFromITransformableExpression() {
    ExpressionSyntaxVisitor
      .ShouldTransformExpressionMember(new object())
      .Should()
      .BeFalse();
  }

  [Fact]
  public void UnwrapClosureWrappedConstantExpression() { }

  [Fact]
  public void UnwrapClosureWrappedConstantExpression_ReturnsWrappedValue() {
    var input = "input";
    Expression<Func<int>> expr = () => input.Length;
    var sut = new ExpressionSyntaxVisitor();

    var propertyExpression = (MemberExpression)expr.Body;
    var closureWrappedValueAccessExpression = (MemberExpression)propertyExpression.Expression!;
    var closureWrappedInstance = (ConstantExpression)closureWrappedValueAccessExpression.Expression!;
    var closureWrappedValueAccessor = (FieldInfo)closureWrappedValueAccessExpression.Member!;

    var r = sut.UnwrapClosureWrappedConstantExpression(closureWrappedInstance, closureWrappedValueAccessor);

    r.Value.Should().Be(input);
  }

  [Fact]
  public void IsClosureWrappedConstantExpression_ReturnsFalse_WhenExpressionHasNoClosureWrappedValue() { }

  [Fact]
  public void IsClosureWrappedConstantExpression_ReturnsTrue_WhenExpressionHasClosureWrappedValue() { }

  [Fact]
  public void IsClosureWrappedConstantExpression() { }

  private class ImplementsITransformMemberExpression : ITransformMemberExpression {
    public string BoundValue { get; set; }
    public string NonBoundValue { get; set; }

    public Expression TransformMemberAccess(Expression target, MemberInfo member) {
      if (member.Name == nameof(BoundValue)) {
        return Expression.Variable(typeof(string), "Bar");
      }

      return Expression.MakeMemberAccess(target, member);
    }
  }
}
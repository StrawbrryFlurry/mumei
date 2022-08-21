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
  public void Clone_ReturnsNewExpressionSyntaxWithSameExpression() {
    var sut = new ExpressionSyntax(() => 10 + 20);

    var clone = sut.Clone();

    clone.ToString().Should().Be(sut.ToString());
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
    var closureVariable = Expression.Variable(typeof(string), "Bar");
    var sut = new ExpressionSyntax(() => closureVariable.Name == "Bar");

    sut.ParseExpressionToSyntaxString().Should().Be("(Bar.Name == \"Bar\")");
  }

  [Fact]
  public void ParseExpressionToSyntaxString_KeepsMemberExpressionAsIs_WhenTargetIsNoExpression() {
    var closureVariable = "Foo";
    var sut = new ExpressionSyntax(() => closureVariable.Length == 5);

    sut.ParseExpressionToSyntaxString().Should().Be("(\"Foo\".Length == 5)");
  }

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
using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

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
    var closureVariable = new ImplementsIValueHolder<string> { Identifier = "Bar" };
    var sut = new ExpressionSyntax(() => closureVariable.Value == "Bar");

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Bar == \"Bar\"");
  }

  [Fact]
  public void ParseExpressionToSyntaxString_KeepsMemberExpressionAsIs_WhenTargetDoesNotImplementITransformMember() {
    var closureVariable = Expression.Variable(typeof(string), "Bar");
    var sut = new ExpressionSyntax(() => closureVariable.Name == "Bar");

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("Bar.Name == \"Bar\"");
  }

  [Fact]
  public void ParseExpressionToSyntaxString_KeepsMemberExpressionAsIs_WhenTargetIsNoExpression() {
    var closureVariable = "Foo";
    var sut = new ExpressionSyntax(() => closureVariable.Length == 5);

    sut.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance).Should().Be("\"Foo\".Length == 5");
  }

  private class ImplementsIValueHolder<T> : IValueHolderSyntax<T> {
    public string Identifier { get; set; }
    public T Value { get; set; }
  }
}
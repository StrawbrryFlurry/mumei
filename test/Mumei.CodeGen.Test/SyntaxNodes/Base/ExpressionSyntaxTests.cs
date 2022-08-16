using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class ExpressionSyntaxTests {
  [Fact]
  public void ToString_ReturnsExpressionAsString() {
    var sut = new ExpressionSyntax(Expression.UnaryPlus(Expression.Constant(1)));

    sut.ToString().Should().Be("+1");
  }
}
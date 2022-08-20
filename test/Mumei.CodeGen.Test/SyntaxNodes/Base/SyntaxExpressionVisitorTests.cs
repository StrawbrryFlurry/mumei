using System.Linq.Expressions;
using System.Reflection;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class SyntaxExpressionVisitorTests {
  [Fact]
  public void UnwrapClosureWrappedConstantExpression_ReturnsWrappedValue() {
    var input = "input";
    Expression<Func<int>> expr = () => input.Length;
    var sut = new SyntaxExpressionVisitor();

    var propertyExpression = (MemberExpression)expr.Body;
    var closureWrappedValueAccessExpression = (MemberExpression)propertyExpression.Expression!;
    var closureWrappedInstance = (ConstantExpression)closureWrappedValueAccessExpression.Expression!;
    var closureWrappedValueAccessor = (FieldInfo)closureWrappedValueAccessExpression.Member!;

    var r = sut.UnwrapClosureWrappedConstantExpression(closureWrappedInstance, closureWrappedValueAccessor);

    r.Value.Should().Be(input);
  }

  [Fact]
  public void IsClosureWrappedConstantExpression_ReturnsFalse_WhenExpressionHasNoClosureWrappedValue() {
    Expression<Func<int>> expr = () => "input".Length;

    var sut = new SyntaxExpressionVisitor();

    sut.IsClosureWrappedConstantExpression((MemberExpression)expr.Body, out _).Should().BeFalse();
  }

  [Fact]
  public void IsClosureWrappedConstantExpression_ReturnsTrue_WhenExpressionHasClosureWrappedValue() {
    var input = "input";
    Expression<Func<string>> expr = () => input;

    var sut = new SyntaxExpressionVisitor();

    sut.IsClosureWrappedConstantExpression((MemberExpression)expr.Body, out _).Should().BeTrue();
  }
}
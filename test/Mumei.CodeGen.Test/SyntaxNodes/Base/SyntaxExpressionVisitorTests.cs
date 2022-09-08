using System.Linq.Expressions;
using System.Reflection;
using Mumei.CodeGen.Expressions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Base;

public class SyntaxExpressionVisitorTests {
  [Fact]
  public void UnwrapClosureWrappedConstantExpression_ReturnsWrappedValue() {
    var input = "input";
    Expression<Func<int>> expr = () => input.Length;
    var sut = new SyntaxExpressionVisitor(new SyntaxTypeContext());

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

    var sut = new SyntaxExpressionVisitor(new SyntaxTypeContext());

    sut.IsClosureWrappedConstantExpression((MemberExpression)expr.Body, out _).Should().BeFalse();
  }

  [Fact]
  public void IsClosureWrappedConstantExpression_ReturnsTrue_WhenExpressionHasClosureWrappedValue() {
    var input = "input";
    Expression<Func<string>> expr = () => input;

    var sut = new SyntaxExpressionVisitor(new SyntaxTypeContext());

    sut.IsClosureWrappedConstantExpression((MemberExpression)expr.Body, out _).Should().BeTrue();
  }

  [Fact]
  public void VisitConstant_ReturnsTypeExpression_WhenConstantIsTypeExpression() {
    var input = Expression.Constant(typeof(int));
    var sut = new SyntaxExpressionVisitor(new SyntaxTypeContext());

    var result = (TypeExpression)sut.Visit(input);

    result.Should().BeOfType<TypeExpression>();
    result.Type.Should().Be<Type>();
    result.ExpressionType.Should().Be<int>();
  }

  [Fact]
  public void VisitConstant_AddsUsedTypeNamespaceToWriter_WhenConstantExpressionIsConvertedToTypeExpression() {
    var input = Expression.Constant(typeof(int));
    var ctx = new SyntaxTypeContext();
    var sut = new SyntaxExpressionVisitor(ctx);

    sut.Visit(input);

    ctx.UsedNamespaces.Should().Contain(typeof(int).Namespace);
  }
}
using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class ReturnStatementSyntaxTests {
  [Fact]
  public void WriteAsSyntax_WritesReturnExpression_WhenReturnValueIsNull() {
    var sut = new ReturnStatementSyntax();

    sut.WriteSyntaxAsString().Should().Be("return;");
  }

  [Fact]
  public void WriteAsSyntax_WritesReturnExpressionWithValue_WhenValueIsExpression() {
    var sut = new ReturnStatementSyntax(Expression.Constant("Hi!"));

    sut.WriteSyntaxAsString().Should().Be("return \"Hi!\";");
  }

  [Fact]
  public void WriteAsSyntax_WritesReturnExpressionNullAsValue_WhenValueIsConstantExpressionWithNull() {
    var sut = new ReturnStatementSyntax(Expression.Constant(null));

    sut.WriteSyntaxAsString().Should().Be("return null;");
  }

  [Fact]
  public void WriteAsSyntax_WritesReturnExpressionWithExpressionValue_WhenExpressionIsCustomExpression() {
    var variable = new VariableExpressionSyntax(typeof(string), "foo");
    var sut = new ReturnStatementSyntax(variable);

    sut.WriteSyntaxAsString().Should().Be("return foo;");
  }

  [Fact]
  public void Clone_ReturnsNewInstance_WithNoParent() {
    var variable = new VariableExpressionSyntax(typeof(string), "foo");
    var sut = new ReturnStatementSyntax(variable);

    var clone = sut.Clone<ReturnStatementSyntax>();

    clone.Parent.Should().BeNull();
  }

  [Fact]
  public void Clone_ReturnsNewInstance_WithClonedValue() {
    var variable = new VariableExpressionSyntax(typeof(string), "foo");
    var sut = new ReturnStatementSyntax(variable);

    var clone = sut.Clone<ReturnStatementSyntax>();

    clone.Value.Should().BeOfType<VariableExpressionSyntax>();
    clone.Value!.Parent.Should().Be(clone);
  }
}
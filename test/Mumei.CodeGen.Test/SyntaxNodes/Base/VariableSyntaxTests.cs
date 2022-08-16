using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class VariableSyntaxTests {
  [Fact]
  public void VariableSyntax_WriteAsSyntax_WritesVariableNameAsString() {
    var sut = new VariableExpressionSyntax<string>("Test");

    WriteSyntaxAsString(sut).Should().Be("Test");
  }

  [Fact]
  public void VariableDeclarationSyntax_WriteAsSyntax_WritesVariableTypeAndName_WhenVariableDoesNotHavInitializer() {
    var sut = new VariableDeclarationSyntax(typeof(string), "Test");

    WriteSyntaxAsString(sut).Should().Be("String Test;");
  }

  [Fact]
  public void
    VariableDeclarationSyntax_WriteAsSyntax_WritesVariableTypeNameAndInitializer_WhenVariableHasInitializer() {
    var sut = new VariableDeclarationSyntax(typeof(string), "Test", Expression.Constant(10));

    WriteSyntaxAsString(sut).Should().Be("String Test = 10;");
  }
}
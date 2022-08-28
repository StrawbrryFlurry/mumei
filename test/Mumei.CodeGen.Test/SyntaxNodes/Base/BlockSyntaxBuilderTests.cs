using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class BlockSyntaxBuilderTests {
  [Fact]
  public void VariableDeclaration_ReturnsBoxedNullValueForPrimitive_WhenNoValueIsSpecified() {
    var sut = new BlockSyntaxBuilder();

    var v = sut.VariableDeclaration(typeof(int), "Test");

    v.Value.Should().BeNull();
  }

  [Fact]
  public void VariableDeclaration_AddsVariableDeclarationToStatementList() {
    var sut = new BlockSyntaxBuilder();

    sut.VariableDeclaration(typeof(string), "Test");
    var declaration = sut.Statements.OfType<VariableDeclarationStatementSyntax>().First();

    declaration.Identifier.Should().Be("Test");
    declaration.Type.Should().Be<string>();
  }

  [Fact]
  public void VariableDeclaration_AddsVariableDeclarationToStatementList_WhenDeclarationHasInitializer() {
    var sut = new BlockSyntaxBuilder();

    sut.VariableDeclaration(typeof(string), "Test", Expression.Constant(42));
    var declaration = sut.Statements.OfType<VariableDeclarationStatementSyntax>().First();

    declaration.Initializer!.ToString().Should().Be("42");
  }

  [Fact]
  public void VariableDeclaration_T_ReturnsVariableExpressionWithDefaultValue() {
    var sut = new BlockSyntaxBuilder();

    var v = sut.VariableDeclaration<string>("Test");

    v.Value.Should().BeNull();
  }

  [Fact]
  public void VariableDeclaration_T_AddsVariableDeclarationToStatementList() {
    var sut = new BlockSyntaxBuilder();

    sut.VariableDeclaration<string>("Test");
    var declaration = sut.Statements.OfType<VariableDeclarationStatementSyntax>().First();

    declaration.Identifier.Should().Be("Test");
    declaration.Type.Should().Be<string>();
  }

  [Fact]
  public void VariableDeclaration_T_AddsVariableDeclarationToStatementList_WhenDeclarationHasInitializer() {
    var sut = new BlockSyntaxBuilder();

    sut.VariableDeclaration<string>("Test", Expression.Constant(42));
    var declaration = sut.Statements.OfType<VariableDeclarationStatementSyntax>().First();

    declaration.Initializer!.ToString().Should().Be("42");
  }

  [Fact]
  public void Assign_AddsAssignmentToStatementList_WhenValueIsObject() {
    var sut = new BlockSyntaxBuilder();

    var variable = new VariableExpressionSyntax<string>("foo");
    sut.Assign(variable, "Bar");
    var assignment = sut.Statements.OfType<ExpressionStatementSyntax>().First();

    WriteSyntaxAsString(assignment).Should().Be("foo = \"Bar\";");
  }

  [Fact]
  public void Assign_AddsAssignmentToStatementList_WhenValueIsExpressionSyntax() {
    var sut = new BlockSyntaxBuilder();

    var variable = new VariableExpressionSyntax<string>("foo");
    var value = new VariableExpressionSyntax<string>("bar");

    sut.Assign(variable, value);
    var assignment = sut.Statements.OfType<ExpressionStatementSyntax>().First();

    WriteSyntaxAsString(assignment).Should().Be("foo = bar;");
  }

  [Fact]
  public void Assign_AddsAssignmentToStatementList_WhenValueIsExpression() {
    var sut = new BlockSyntaxBuilder();
    var stringSubstringMethod = typeof(string).GetMethod(nameof(string.Substring), new[] { typeof(int) })!;

    var variable = new VariableExpressionSyntax<string>("foo");
    var value = new VariableExpressionSyntax<string>("bar");
    var callExpression = Expression.Call(
      value,
      stringSubstringMethod,
      Expression.Constant(1)
    );

    sut.Assign(variable, callExpression);
    var assignment = sut.Statements.OfType<ExpressionStatementSyntax>().First();

    WriteSyntaxAsString(assignment).Should().Be("foo = bar.Substring(1);");
  }

  [Fact]
  public void Build_CreatesNewBlockSyntaxWithAllStatementsAddedInTheBlock() {
    var sut = new BlockSyntaxBuilder();

    sut.Statement(Expression.Empty());
    sut.Statement(Expression.Empty());
    sut.Statement(Expression.Empty());

    var block = sut.Build();

    block.Statements.Should().HaveCount(3);
  }

  [Fact]
  public void Build_SetsParentOfBlock_WhenParentIsSpecified() {
    var parent = new BlockSyntax();
    var sut = new BlockSyntaxBuilder(parent);

    var block = sut.Build();

    block.Parent.Should().Be(parent);
  }
}
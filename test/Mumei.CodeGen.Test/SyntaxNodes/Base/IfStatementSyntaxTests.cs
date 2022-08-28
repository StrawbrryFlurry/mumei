using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class IfStatementSyntaxTests {
  [Fact]
  public void Ctor_SetsParentOfConditionAndBlockToSelf() {
    var condition = new ExpressionSyntax(() => true);
    var body = new BlockSyntax();

    var sut = new IfStatementSyntax(condition, body);

    condition.Parent.Should().Be(sut);
    body.Parent.Should().Be(sut);
  }

  [Fact]
  public void DefineElse_SetsParentOfBody() {
    var body = new BlockSyntax();

    var sut = new IfStatementSyntax(new ExpressionSyntax(() => true), new BlockSyntax());
    sut.DefineElse(body);

    body.Parent.Should().Be(sut);
  }

  [Fact]
  public void DefineElse_SetsElseClauseInStatement() {
    var body = new BlockSyntax();

    var sut = new IfStatementSyntax(new ExpressionSyntax(() => true), new BlockSyntax());
    sut.DefineElse(body);

    sut.ElseClause.Should().Be(body);
  }

  [Fact]
  public void AddElseIf_SetsStatementAsParentOfConditionAndBody() {
    var condition = new ExpressionSyntax(() => false);
    var body = new BlockSyntax();

    var sut = new IfStatementSyntax(new ExpressionSyntax(() => true), new BlockSyntax());
    sut.AddElseIf(condition, body);

    // Skip the first one, as it's the if statement itself
    var elseIfClause = sut.Clauses.Skip(1).First();

    elseIfClause.Item1.Parent.Should().Be(sut);
    elseIfClause.Item2.Parent.Should().Be(sut);
  }

  [Fact]
  public void AddElseIf_AddsClauseToInternalList() {
    var condition = new ExpressionSyntax(() => false);
    var body = new BlockSyntax();

    var sut = new IfStatementSyntax(new ExpressionSyntax(() => true), new BlockSyntax());
    sut.AddElseIf(condition, body);

    // Skip the first one, as it's the if statement itself
    var elseIfClause = sut.Clauses.Skip(1).First();

    elseIfClause.Item1.Should().Be(condition);
    elseIfClause.Item2.Should().Be(body);
  }

  [Fact]
  public void WriteAsSyntax_WritesStandaloneIfStatement() {
    var sut = new IfStatementSyntax(
      new ExpressionSyntax(() => true),
      b => b.VariableDeclaration<string>("foo", Expression.Constant(42))
    );

    WriteSyntaxAsString(sut).Should().Be(
      Line("if(true) {") +
      IndentedLine("String foo = 42;", 1) +
      "}");
  }

  [Fact]
  public void WriteAsSyntax_WritesElseStatement() {
    var sut = new IfStatementSyntax(
      new ExpressionSyntax(() => true),
      b => b.VariableDeclaration<string>("foo", Expression.Constant(42))
    );

    sut.DefineElse(b => b.VariableDeclaration<string>("bar", Expression.Constant(42)));

    WriteSyntaxAsString(sut).Should().Be(
      Line("if(true) {") +
      IndentedLine("String foo = 42;", 1) +
      Line("} else {") +
      IndentedLine("String bar = 42;", 1) +
      "}"
    );
  }

  [Fact]
  public void WriteAsSyntax_WritesElseIfStatements() {
    var sut = new IfStatementSyntax(
      new ExpressionSyntax(() => true),
      b => b.VariableDeclaration<string>("foo", Expression.Constant(42))
    );

    sut.AddElseIf(() => false, b => b.VariableDeclaration<string>("bar", Expression.Constant(42)));

    WriteSyntaxAsString(sut).Should().Be(
      Line("if(true) {") +
      IndentedLine("String foo = 42;", 1) +
      Line("} else if(false) {") +
      IndentedLine("String bar = 42;", 1) +
      "}"
    );
  }

  [Fact]
  public void WriteAsSyntax_WritesElseAndElseIfStatements() {
    var sut = new IfStatementSyntax(
      new ExpressionSyntax(() => true),
      b => b.VariableDeclaration<string>("foo", Expression.Constant(42))
    );

    sut.AddElseIf(() => false, b => b.VariableDeclaration<string>("bar", Expression.Constant(42)));
    sut.AddElseIf(() => false, b => b.VariableDeclaration<string>("baz", Expression.Constant(42)));
    sut.DefineElse(b => b.VariableDeclaration<string>("qux", Expression.Constant(42)));

    WriteSyntaxAsString(sut).Should().Be(
      Line("if(true) {") +
      IndentedLine("String foo = 42;", 1) +
      Line("} else if(false) {") +
      IndentedLine("String bar = 42;", 1) +
      Line("} else if(false) {") +
      IndentedLine("String baz = 42;", 1) +
      Line("} else {") +
      IndentedLine("String qux = 42;", 1) +
      "}"
    );
  }

  [Fact]
  public void Clone_CreatesNewInstance() {
    var condition = new ExpressionSyntax(() => true);
    var body = new BlockSyntax();

    var sut = new IfStatementSyntax(condition, body);

    var clone = sut.Clone();

    clone.Should().NotBeSameAs(sut);
  }

  [Fact]
  public void Clone_CopiesConditionAndBlock() {
    var condition = new ExpressionSyntax(() => true);
    var body = new BlockSyntax();
    var sut = new IfStatementSyntax(condition, body);

    var clone = sut.Clone<IfStatementSyntax>();
    var (conditionClone, bodyClone) = clone.Clauses.First();

    conditionClone.Should().NotBeSameAs(condition);
    conditionClone.Parent.Should().Be(clone);
    bodyClone.Should().NotBeSameAs(body);
    bodyClone.Parent.Should().Be(clone);
  }

  [Fact]
  public void Clone_CopiesAllClauses() {
    var condition = new ExpressionSyntax(() => true);
    var body = new BlockSyntax();

    var sut = new IfStatementSyntax(condition, body);

    sut.AddElseIf(() => false, builder => { });
    sut.AddElseIf(() => false, builder => { });
    sut.AddElseIf(() => false, builder => { });

    var clone = sut.Clone<IfStatementSyntax>();

    clone.Clauses.Should().HaveCount(4);

    foreach (var cloneClause in clone.Clauses) {
      cloneClause.Item1.Parent.Should().Be(clone);
      cloneClause.Item2.Parent.Should().Be(clone);
    }
  }
}
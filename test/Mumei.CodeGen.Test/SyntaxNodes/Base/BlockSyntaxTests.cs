using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class BlockSyntaxTests {
  [Fact]
  public void WriteAsSyntax_WritesEmptyBrackets_WhenBlockDoesNotHaveAnyStatements() {
    var sut = new BlockSyntax();

    sut.WriteSyntaxAsString().Should().Be(Line("{") + "}");
  }

  [Fact]
  public void WriteAsSyntax_WritesAllStatementsSeperatedByNewLinesWithIndentation() {
    var sut = new BlockSyntax();

    sut.AddStatement(new VariableDeclarationStatementSyntax(typeof(string), "Foo"));
    sut.AddStatement(new VariableDeclarationStatementSyntax(typeof(string), "Bar"));

    sut.WriteSyntaxAsString().Should().Be(
      Line("{") +
      IndentedLine("String Foo;", 1) +
      IndentedLine("String Bar;", 1) +
      "}");
  }

  [Fact]
  public void AddStatement_SetsParentOfStatement() {
    var sut = new BlockSyntax();
    var statement = new BlockSyntax();

    sut.AddStatement(statement);

    statement.Parent.Should().Be(sut);
  }

  [Fact]
  public void Clone_ReturnsNewBlockWithSameStatements() {
    var sut = new BlockSyntax();
    var statement = new VariableDeclarationStatementSyntax(typeof(string), "Foo");
    sut.AddStatement(statement);

    var clone = (BlockSyntax)sut.Clone();

    clone.Parent.Should().BeNull();
    clone.Statements.OfType<VariableDeclarationStatementSyntax>().First().Identifier.Should().Be("Foo");
  }
}
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class BlockSyntaxTests {
  [Fact]
  public void WriteAsSyntax_WritesEmptyBrackets_WhenBlockDoesNotHaveAnyStatements() {
    var sut = new BlockSyntax();

    WriteSyntaxAsString(sut).Should().Be(Line("{") + "}");
  }

  [Fact]
  public void WriteAsSyntax_WritesAllStatementsSeperatedByNewLinesWithIndentation() {
    var sut = new BlockSyntax();

    sut.AddStatement(new VariableDeclarationSyntax(typeof(string), "Foo"));
    sut.AddStatement(new VariableDeclarationSyntax(typeof(string), "Bar"));

    WriteSyntaxAsString(sut).Should().Be(
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
}
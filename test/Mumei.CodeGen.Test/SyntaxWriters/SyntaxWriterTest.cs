using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxWriters;

public class SyntaxWriterTest {
  [Fact]
  public void GetIndent_ReturnsEmptyString_WhenIndentIsZero() {
    var sut = new SyntaxWriter();

    var indent = sut.GetIndent();

    indent.Should().BeEmpty();
  }

  [Fact]
  public void GetIndent_ReturnsStringWithTwoSpaces_WhenIndentIsOne() {
    var sut = new SyntaxWriter();

    sut.Indent();
    var indent = sut.GetIndent();

    indent.Should().Be(IndentationString(1));
  }

  [Fact]
  public void Indent_IncreasesTheIndentationByOne() {
    var sut = new SyntaxWriter();

    sut.IndentLevelLevel.Should().Be(0);

    sut.Indent();

    sut.IndentLevelLevel.Should().Be(1);
  }

  [Fact]
  public void UnIndent_DecreasesTheIndentationByOne() {
    var sut = new SyntaxWriter();

    sut.Indent();

    sut.IndentLevelLevel.Should().Be(1);

    sut.UnIndent();

    sut.IndentLevelLevel.Should().Be(0);
  }

  [Fact]
  public void UnIndent_DoesNothing_WhenIndentLevelIsZero() {
    var sut = new SyntaxWriter();

    sut.IndentLevelLevel.Should().Be(0);

    sut.UnIndent();

    sut.IndentLevelLevel.Should().Be(0);
  }

  [Fact]
  public void IndentLevel_DoesNotAllowValuesBelowZero_WhenTheyAreSet() {
    var sut = new SyntaxWriter();

    sut.IndentLevelLevel = -1;

    sut.IndentLevelLevel.Should().Be(0);
  }

  [Fact]
  public void SetIndentLevel_SetsIndentLevel() {
    var sut = new SyntaxWriter();

    sut.IndentLevelLevel.Should().Be(0);

    sut.SetIndentLevel(1);

    sut.IndentLevelLevel.Should().Be(1);
  }

  [Fact]
  public void SettingIndentLevel_RecalculatesTheIndent_WhenItIsCalled() {
    var sut = new SyntaxWriter();

    sut.IndentLevelLevel = 2;

    sut.GetIndent().Should().Be(IndentationString(2));
  }

  [Fact]
  public void WriteLine_AddsLineToCode() {
    var sut = new SyntaxWriter();

    sut.WriteLine("line");
    var code = sut.ToSyntax();

    code.Should().Be(Line("line"));
  }

  [Fact]
  public void WriteLine_AddsLineToCodeWithIndent_WhenIndentIsGreaterThanZero() {
    var sut = new SyntaxWriter();

    sut.WriteLine("class Foo {");

    sut.Indent();

    sut.WriteLine("public void Bar() {");
    sut.WriteLine("}");

    sut.UnIndent();

    sut.WriteLine("}");

    var code = sut.ToSyntax();

    var expected = Line("class Foo {") +
                   IndentedLine("public void Bar() {", 1) +
                   IndentedLine("}", 1) +
                   Line("}");

    code.Should().Be(expected);
  }

  [Fact]
  public void WriteNewLine_AddsNewLineToTheEndOfTheLine() {
    var sut = new SyntaxWriter();

    sut.Write("class Foo ");
    sut.WriteLineEnd("{");
    var code = sut.ToSyntax();

    code.Should().Be(Line("class Foo {"));
  }

  [Fact]
  public void Write_AppendsTextToTheCurrentLine() {
    var sut = new SyntaxWriter();

    sut.Write("class Foo ");
    var code = sut.ToSyntax();

    code.Should().Be("class Foo ");
  }

  [Fact]
  public void Write_AppendsSyntaxIdentifierToLine_WhenArgumentIsSyntax() {
    var sut = new SyntaxWriter();

    sut.Write(new FooSyntax("Foo"));
    var code = sut.ToSyntax();

    code.Should().Be("Foo");
  }

  private class FooSyntax : Syntax {
    public FooSyntax(string name) : base(name) {
    }

    public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    }
  }
}
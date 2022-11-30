using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.SyntaxWriters;

public class SeparationStrategyTests {
  [Fact]
  public void NewLineSeparationStrategy_WritesNewLineToWriter() {
    var writer = new SyntaxWriter();

    writer.Write("foo");
    SeparationStrategy.NewLine.WriteSeparator(writer);
    writer.Write("bar");

    writer.ToSyntax().Should().Be(Line("foo") + "bar");
  }

  [Fact]
  public void NoneSeparationStrategy_WritesNothingToWriter() {
    var writer = new SyntaxWriter();

    writer.Write("foo");
    SeparationStrategy.None.WriteSeparator(writer);
    writer.Write("bar");

    writer.ToSyntax().Should().Be("foobar");
  }

  [Fact]
  public void NewLineSeparationStrategy_WritesSpaceToWriter() {
    var writer = new SyntaxWriter();

    writer.Write("foo");
    SeparationStrategy.Space.WriteSeparator(writer);
    writer.Write("bar");

    writer.ToSyntax().Should().Be("foo bar");
  }
}
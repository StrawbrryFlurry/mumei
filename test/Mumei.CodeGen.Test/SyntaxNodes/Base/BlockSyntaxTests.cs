using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Base;

public class BlockSyntaxTests {
  public void WriteAsSyntax_() {
    var sut = new BlockSyntax();

    WriteSyntaxAsString(sut).Should().Be(Line("{") + "}");
  }
}
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
}
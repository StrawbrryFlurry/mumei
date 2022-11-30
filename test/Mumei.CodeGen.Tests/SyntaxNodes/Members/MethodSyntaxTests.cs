using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.CodeGen.Tests.SyntaxNodes.Members;

public class MethodSyntaxTests {
  [Fact]
  public void ParseAsExpression() {
    var sut = new MethodSyntax(typeof(string), "Foo");
  }
}
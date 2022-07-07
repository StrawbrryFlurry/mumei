using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubSyntaxBuilder : SyntaxBuilder<StubSyntax> {
  public StubSyntaxBuilder(string name, Syntax? parent = null) : base(name, parent) {
  }

  public override StubSyntax Build() {
    return MakeSyntaxInstance();
  }
}
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubSyntax : Syntax {
  public StubSyntax(string identifier) : base(identifier) {
  }

  public StubSyntax(string identifier, Syntax? parent) : base(identifier, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
  }
}
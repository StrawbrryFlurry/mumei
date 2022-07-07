using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubSyntax : Syntax {
  public StubSyntax(string name) : base(name) {
  }

  public StubSyntax(string name, Syntax? parent) : base(name, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
  }
}
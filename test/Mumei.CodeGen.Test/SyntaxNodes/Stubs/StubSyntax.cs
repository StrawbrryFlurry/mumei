using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubSyntax : Syntax {
  public StubSyntax() { }
  public StubSyntax(Syntax? parent) : base(parent) { }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) { }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}
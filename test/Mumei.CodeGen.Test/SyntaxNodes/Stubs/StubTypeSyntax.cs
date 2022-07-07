using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubTypeSyntax : TypeSyntax {
  public StubTypeSyntax(string name) : base(name) {
  }

  public StubTypeSyntax(string name, Syntax? parent) : base(name, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
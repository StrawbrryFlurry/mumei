using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubTypeSyntax : TypeSyntax {
  public StubTypeSyntax(string identifier) : base(identifier) {
  }

  public StubTypeSyntax(string identifier, Syntax? parent) : base(identifier, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
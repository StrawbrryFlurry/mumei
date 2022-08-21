using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubTypeDeclarationSyntax : TypeDeclarationSyntax {
  public StubTypeDeclarationSyntax(string identifier) : base(identifier) { }

  public StubTypeDeclarationSyntax(string identifier, Syntax? parent) : base(identifier, parent) { }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}
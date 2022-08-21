using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubMemberSyntax : MemberSyntax {
  public StubMemberSyntax(string identifier, Syntax parent) : base(identifier, parent) { }

  public override int Priority { get; } = 10;

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}
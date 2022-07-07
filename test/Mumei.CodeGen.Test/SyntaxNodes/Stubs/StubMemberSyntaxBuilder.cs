using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Stubs;

public class StubMemberSyntaxBuilder : MemberSyntaxBuilder<StubMemberSyntax> {
  public StubMemberSyntaxBuilder(string name, Type type, TypeSyntax parent) : base(name, type, parent) {
  }

  public override StubMemberSyntax Build() {
    return MakeSyntaxInstance();
  }
}
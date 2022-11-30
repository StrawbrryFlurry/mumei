using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.CodeGen.Tests.SyntaxNodes.Members;

public class MemberSyntaxTests {
  private class MemberSyntaxImpl : MemberSyntax {
    public MemberSyntaxImpl(Type type, string identifier, Syntax? parent = null) : base(type, identifier, parent) { }
    protected internal override int Priority { get; }

    public override Syntax Clone() {
      throw new NotImplementedException();
    }
  }
}
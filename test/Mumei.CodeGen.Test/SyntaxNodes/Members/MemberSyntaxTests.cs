using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.Test.SyntaxNodes.Members;

public class MemberSyntaxTests {
  private class MemberSyntaxImpl : MemberSyntax {
    public MemberSyntaxImpl(Type type, string identifier, Syntax? parent = null) : base(type, identifier, parent) { }
    public override int Priority { get; }

    public override Syntax Clone() {
      throw new NotImplementedException();
    }
  }
}
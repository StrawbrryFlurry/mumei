using Mumei.CodeGen.SyntaxBuilders;

namespace Mumei.Test.SyntaxBuilders;

public class MemberSyntaxBuilderTest {
  [Fact]
  public void TestMemberSyntaxBuilder() {
    var builder = new MemberSyntaxBuilderImpl();
    var member = builder.ToString();
  }

  private class MemberSyntaxBuilderImpl : MemberSyntaxBuilder {
  }
}
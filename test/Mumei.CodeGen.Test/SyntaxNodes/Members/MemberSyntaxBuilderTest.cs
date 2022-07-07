using FluentAssertions;
using Mumei.Test.SyntaxNodes.Stubs;

namespace Mumei.Test.SyntaxNodes.Members;

public class MemberSyntaxBuilderTest {
  [Fact]
  public void Add_AddsMemberToParentSyntax() {
    var parent = new StubTypeSyntax("");

    var member = new StubMemberSyntaxBuilder("Foo", typeof(string), parent).Add();

    parent.Members.Should().Contain(m => m.Name == member.Name);
  }
}
using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.Test.SyntaxNodes.Stubs;

namespace Mumei.Test.SyntaxNodes.Members;

public class FieldSyntaxBuilderTest {
  [Fact]
  public void Build_ReturnsFieldWithNameTypeSet() {
    var parent = new StubTypeSyntax("");
    var sut = new FieldSyntaxBuilder(parent, "field", typeof(string)).Build();

    sut.Name.Should().Be("field");
    sut.Type.Should().Be(typeof(string));
  }
}
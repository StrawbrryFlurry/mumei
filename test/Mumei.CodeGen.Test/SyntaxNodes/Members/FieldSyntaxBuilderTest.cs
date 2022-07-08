using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.Test.SyntaxNodes.Stubs;

namespace Mumei.Test.SyntaxNodes.Members;

public class FieldSyntaxBuilderTest {
  [Fact]
  public void Build_ReturnsFieldWithIdentifierTypeSet() {
    var parent = new StubTypeSyntax("");
    var sut = new FieldSyntaxBuilder(parent, "field", typeof(string)).Build();

    sut.Identifier.Should().Be("field");
    sut.Type.Should().Be(typeof(string));
  }

  [Fact]
  public void SetInitialValue_SetsInitializerInField() {
    var sut = new FieldSyntaxBuilder(null!, "field", typeof(string))
              .SetInitialValue("FooBar")
              .Build();

    sut.Initializer.Should().Be("FooBar");
  }
}
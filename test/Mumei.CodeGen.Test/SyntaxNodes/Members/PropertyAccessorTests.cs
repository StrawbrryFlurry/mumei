using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Members;

public class PropertyAccessorTests {
  [Fact]
  public void WriteAsSyntax_WritesAutoGetter_WhenAccessorIsGetterAndHasNoBody() {
    var sut = new PropertyAccessor(PropertyAccessorType.Get, null);

    var writer = new TypeAwareSyntaxWriter(new SyntaxTypeContext());
    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be("get;");
  }
}
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.SyntaxNodes;

public class SyntaxTests {
  [Fact]
  public void SetParent_SetsParentOfSyntaxNode_WhenParentIsNull() {
    var parent = new SyntaxImpl();
    var sut = new SyntaxImpl();

    sut.SetParent(parent);

    sut.Parent.Should().Be(parent);
  }

  [Fact]
  public void SetParent_ThrowsInvalidOperationException_WhenSyntaxHasAParentDefined() {
    var sut = new SyntaxImpl(new SyntaxImpl());

    var action = () => sut.SetParent(new SyntaxImpl());

    action.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void SetParent_SetsTypeContextOfNode_ToContextOfParent() {
    var newParent = new SyntaxImpl();
    var sut = new SyntaxImpl();

    sut.SetParent(newParent);

    sut.PublicTypeContext.Should().Be(newParent.PublicTypeContext);
  }

  private class SyntaxImpl : Syntax {
    public SyntaxImpl(Syntax parent = null!) : base(parent) { }

    public SyntaxTypeContext PublicTypeContext => TypeContext;

    public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
      throw new NotImplementedException();
    }

    public override Syntax Clone() {
      throw new NotImplementedException();
    }
  }
}
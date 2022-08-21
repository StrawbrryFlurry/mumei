using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes;

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

  private class SyntaxImpl : Syntax {
    public SyntaxImpl(Syntax parent = null) : base(parent) { }

    public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
      throw new NotImplementedException();
    }

    public override Syntax Clone() {
      throw new NotImplementedException();
    }
  }
}
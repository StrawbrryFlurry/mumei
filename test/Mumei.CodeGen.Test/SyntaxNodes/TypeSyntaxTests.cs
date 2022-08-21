using System.Runtime.CompilerServices;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes;

public class TypeSyntaxTests {
  [Fact]
  public void WriteAsSyntax_WritesAccessibilityModifiersToWriter_WhenTypeHasVisibilitySet() {
    var writer = new TypeAwareSyntaxWriter(new SyntaxTypeContext());
    var sut = new TypeSyntaxImpl("<identifier>") {
      Visibility = SyntaxVisibility.Abstract | SyntaxVisibility.Public
    };

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be("public abstract <identifier>");
  }

  [Fact]
  public void WriteAttribute_WritesNothing_WhenAttributeListIsEmpty() {
    var writer = new TypeAwareSyntaxWriter(new SyntaxTypeContext());
    var sut = new TypeSyntaxImpl("<identifier>");

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be("<identifier>");
  }

  [Fact]
  public void WriteAsSyntax_WritesAttributesToWriter_WhenAttributeListContainsAttributes() {
    var writer = new TypeAwareSyntaxWriter(new SyntaxTypeContext());
    var sut = new TypeSyntaxImpl("<identifier>") {
      AttributeList = {
        AttributeSyntax.Create<StateMachineAttribute>(),
        AttributeSyntax.Create<AsyncStateMachineAttribute>()
      }
    };

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(
      Line("[StateMachine()]") +
      Line("[AsyncStateMachine()]") +
      "<identifier>"
    );
  }

  private class TypeSyntaxImpl : TypeSyntax {
    public TypeSyntaxImpl(string identifier) : base(identifier) { }

    public override Syntax Clone() {
      throw new NotImplementedException();
    }
  }
}
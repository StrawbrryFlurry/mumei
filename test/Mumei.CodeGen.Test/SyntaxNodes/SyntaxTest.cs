using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxNodes;

public class SyntaxTest {
  [Fact]
  public void GetAttributeSyntax_ReturnsNull_WhenSyntaxHasNoAttributes() {
    var sut = new SyntaxImpl();

    var syntax = sut.GetAttributeSyntax();

    syntax.Should().BeNull();
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingTheAttributeWithNoNewLine_WhenSyntaxHasOneAttributeAndSameLineIsFalse() {
    var sut = new SyntaxImpl {
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };

    var syntax = sut.GetAttributeSyntax(true);

    syntax.Should().Be("[StateMachine()]");
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingTheAttributeWithNewLineAtTheEnd_WhenSyntaxHasOneAttribute() {
    var sut = new SyntaxImpl {
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };

    var syntax = sut.GetAttributeSyntax();

    syntax.Should().Be(Line("[StateMachine()]"));
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingMultipleAttributesOnMultipleLines_WhenSyntaxHasMultipleAttributes() {
    var sut = new SyntaxImpl {
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>(),
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };

    var syntax = sut.GetAttributeSyntax();

    syntax.Should().Be(
      Line("[StateMachine()]") +
      Line("[StateMachine()]")
    );
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingMultipleAttributesOnSingleLine_WhenSyntaxHasMultipleAttributes() {
    var sut = new SyntaxImpl {
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>(),
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };

    var syntax = sut.GetAttributeSyntax(true);

    syntax.Should().Be("[StateMachine()] [StateMachine()]");
  }

  private class SyntaxImpl : Syntax {
    public SyntaxImpl() : base("") {
    }

    public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    }
  }
}
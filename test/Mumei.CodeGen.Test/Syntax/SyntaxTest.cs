using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.Syntax;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;
using SyntaxClass = Mumei.CodeGen.Syntax.Syntax;

namespace Mumei.Test.Syntax;

public class SyntaxTest {
  [Fact]
  public void GetAttributeSyntax_ReturnsNull_WhenSyntaxHasNoAttributes() {
    var config = new SyntaxConfiguration("");

    var sut = new SyntaxImpl(config);
    var syntax = sut.GetAttributeSyntax();

    syntax.Should().BeNull();
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingTheAttributeWithNoNewLine_WhenSyntaxHasOneAttributeAndSameLineIsFalse() {
    var config = new SyntaxConfiguration("") {
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };

    var sut = new SyntaxImpl(config);
    var syntax = sut.GetAttributeSyntax(true);

    syntax.Should().Be("[StateMachine()]");
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingTheAttributeWithNewLineAtTheEnd_WhenSyntaxHasOneAttribute() {
    var config = new SyntaxConfiguration("") {
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };

    var sut = new SyntaxImpl(config);
    var syntax = sut.GetAttributeSyntax();

    syntax.Should().Be(Line("[StateMachine()]"));
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingMultipleAttributesOnMultipleLines_WhenSyntaxHasMultipleAttributes() {
    var config = new SyntaxConfiguration("") {
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>(),
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };

    var sut = new SyntaxImpl(config);
    var syntax = sut.GetAttributeSyntax();

    syntax.Should().Be(
      Line("[StateMachine()]") +
      Line("[StateMachine()]")
    );
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingMultipleAttributesOnSingleLine_WhenSyntaxHasMultipleAttributes() {
    var config = new SyntaxConfiguration("") {
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>(),
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };

    var sut = new SyntaxImpl(config);
    var syntax = sut.GetAttributeSyntax(true);

    syntax.Should().Be("[StateMachine()] [StateMachine()]");
  }

  private class SyntaxImpl : SyntaxClass {
    public SyntaxImpl(SyntaxConfiguration config) : base(config) {
    }

    public override string WriteAsSyntax(TypeAwareSyntaxWriter writer) {
      return "";
    }
  }
}
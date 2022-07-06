using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxBuilders;

public class MemberSyntaxBuilderTest {
  [Fact]
  public void GetAttributeString_ReturnsAttributeAsString_WhenAttributeHasNoArguments() {
    var builder = new MemberSyntaxBuilderImpl(0, new SyntaxTypeContext());

    builder.AddAttribute<StateMachineAttribute>();
    var attributeString = builder.GetAttributeString();

    attributeString.Should().Be(Line("[StateMachine()]"));
  }

  [Fact]
  public void GetAttributeString_ReturnsAttributeWithArgumentsAsString_WhenAttributeHasArguments() {
    var builder = new MemberSyntaxBuilderImpl(0, new SyntaxTypeContext());

    builder.AddAttribute<StateMachineAttribute>("StateMachine");
    var attributeString = builder.GetAttributeString();

    attributeString.Should().Be(Line("[StateMachine(\"StateMachine\")]"));
  }

  [Fact]
  public void GetAttributeString_ReturnsFormattedAttributeList_WhenMemberHasMultipleAttributes() {
    var builder = new MemberSyntaxBuilderImpl(0, new SyntaxTypeContext());

    builder.AddAttribute<StateMachineAttribute>("StateMachine");
    builder.AddAttribute<StateMachineAttribute>("StateMachine");
    var attributeString = builder.GetAttributeString();

    attributeString.Should().Be(
      Line("[StateMachine(\"StateMachine\")]") +
      Line("[StateMachine(\"StateMachine\")]")
    );
  }

  [Fact]
  public void GetAttributeString_WritesAttributesWithCorrectIndentation_WhenIndentLevelIsSpecified() {
    var builder = new MemberSyntaxBuilderImpl(2, new SyntaxTypeContext());

    builder.AddAttribute<StateMachineAttribute>("StateMachine");
    var attributeString = builder.GetAttributeString();

    // attributeString.Should().Be(IndentedLine("[StateMachine(\"StateMachine\")]", 2));
  }

  private class MemberSyntaxBuilderImpl : MemberSyntaxBuilder {
    public MemberSyntaxBuilderImpl(int indentLevel, SyntaxTypeContext ctx) : base(indentLevel, ctx) {
    }

    public override string Build() {
      return null!;
    }
  }
}
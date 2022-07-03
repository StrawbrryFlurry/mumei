using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxWriters;

public class AttributeSyntaxWriterTest {
  [Fact]
  public void WriteAttribute_WritesAttributeWithNoArgumentsToTheBuffer() {
    var sut = new AttributeSyntaxWriter(new WriterTypeContext());

    sut.WriteAttribute(typeof(StateMachineAttribute));

    sut.ToString().Should().Be(Line("[StateMachine()]"));
  }

  [Fact]
  public void WriteAttribute_WritesAttributeWithSingleArgumentToTheBuffer() {
    var sut = new AttributeSyntaxWriter(new WriterTypeContext());

    sut.WriteAttribute(typeof(StateMachineAttribute), "StateMachine");

    sut.ToString().Should().Be(Line("[StateMachine(\"StateMachine\")]"));
  }

  [Fact]
  public void WriteAttribute_WritesAttributeWithMultipleArgumentsToTheBuffer() {
    var sut = new AttributeSyntaxWriter(new WriterTypeContext());

    sut.WriteAttribute(typeof(StateMachineAttribute), "StateMachine", typeof(IEnumerable<string>));

    sut.ToString().Should().Be(Line("[StateMachine(\"StateMachine\", typeof(IEnumerable<String>))]"));
  }

  [Fact]
  public void WriteAttribute_AddsAttributeTypeToNamespaceCache_WhenAttributeHasNamespace() {
    var ctx = new WriterTypeContext();
    var sut = new AttributeSyntaxWriter(ctx);

    sut.WriteAttribute(typeof(StateMachineAttribute));

    ctx.UsedNamespaces.First().Should().Be("System.Runtime.CompilerServices");
  }
}
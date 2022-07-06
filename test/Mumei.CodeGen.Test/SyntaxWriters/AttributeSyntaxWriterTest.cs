using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxWriters;

public class AttributeSyntaxWriterTest {
  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithNoArguments() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>();

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be(Line("[StateMachine()]"));
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithSingleArgument() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>("StateMachine");

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be(Line("[StateMachine(\"StateMachine\")]"));
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithMultipleArguments() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>("StateMachine", typeof(IEnumerable<string>));

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be(Line("[StateMachine(\"StateMachine\", typeof(IEnumerable<String>))]"));
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithNamedArgument() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>(
      new Dictionary<NamedAttributeParameter, object> {{"Parameter", "StateMachine"}}
    );

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be(Line("[StateMachine(Parameter: \"StateMachine\")]"));
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithNamedFieldArgument() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>(
      new Dictionary<NamedAttributeParameter, object> {
        {new NamedAttributeParameter {Name = "Field", IsField = true}, "StateMachine"}
      }
    );

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be(Line("[StateMachine(Field = \"StateMachine\")]"));
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithAllArgumentTypes() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>(
      new Dictionary<NamedAttributeParameter, object> {
        {"NamedParameter", "State"},
        {new NamedAttributeParameter {Name = "NamedField", IsField = true}, "Machine"}
      },
      "StateMachine"
    );

    sut.WriteAttribute(attribute);

    sut.ToSyntax()
       .Should()
       .Be(Line("[StateMachine(\"StateMachine\", NamedParameter: \"State\", NamedField = \"Machine\")]"));
  }

  [Fact]
  public void WriteAttribute_AddsAttributeTypeToNamespaceCache_WhenAttributeHasNamespace() {
    var ctx = new SyntaxTypeContext();
    var sut = new AttributeSyntaxWriter(ctx);
    var attribute = AttributeUsage.Create<StateMachineAttribute>();

    sut.WriteAttribute(attribute);

    ctx.UsedNamespaces.First().Should().Be("System.Runtime.CompilerServices");
  }
}
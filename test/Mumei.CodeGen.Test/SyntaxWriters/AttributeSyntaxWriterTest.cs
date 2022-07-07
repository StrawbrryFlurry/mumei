using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxWriters;

public class AttributeSyntaxWriterTest {
  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithNoArguments() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>();

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be("[StateMachine()]");
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithSingleArgument() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>("StateMachine");

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be("[StateMachine(\"StateMachine\")]");
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithMultipleArguments() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>("StateMachine", typeof(IEnumerable<string>));

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be("[StateMachine(\"StateMachine\", typeof(IEnumerable<String>))]");
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithNamedArgument() {
    var sut = new AttributeSyntaxWriter(new SyntaxTypeContext());
    var attribute = AttributeUsage.Create<StateMachineAttribute>(
      new Dictionary<NamedAttributeParameter, object> {{"Parameter", "StateMachine"}}
    );

    sut.WriteAttribute(attribute);

    sut.ToSyntax().Should().Be("[StateMachine(Parameter: \"StateMachine\")]");
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

    sut.ToSyntax().Should().Be("[StateMachine(Field = \"StateMachine\")]");
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
       .Be("[StateMachine(\"StateMachine\", NamedParameter: \"State\", NamedField = \"Machine\")]");
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
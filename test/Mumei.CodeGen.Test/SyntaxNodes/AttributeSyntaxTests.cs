using System.Runtime.CompilerServices;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using Mumei.Test.SyntaxNodes.Stubs;

namespace Mumei.Test.SyntaxNodes;

public class AttributeSyntaxTests {
  public readonly SyntaxTypeContext _ctx = new();
  public readonly Syntax _parent = new StubSyntax();

  [Fact]
  public void Clone_() {
    var sut = new AttributeSyntax(typeof(StateMachineAttribute));
    var clone = sut.Clone<AttributeSyntax>();

    clone.Type.Should().Be(sut.Type);
    clone.NamedArguments.Should().BeSameAs(sut.NamedArguments);
    clone.PositionalArguments.Should().BeSameAs(sut.PositionalArguments);
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithNoArguments() {
    var sut = AttributeSyntax.Create<StateMachineAttribute>();
    var writer = new TypeAwareSyntaxWriter(_ctx);

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be("[StateMachine()]");
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithSingleArgument() {
    var sut = AttributeSyntax.Create<StateMachineAttribute>("StateMachine");
    var writer = new TypeAwareSyntaxWriter(_ctx);

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be("[StateMachine(\"StateMachine\")]");
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithMultipleArguments() {
    var sut = AttributeSyntax.Create<StateMachineAttribute>("StateMachine", typeof(IEnumerable<string>));
    var writer = new TypeAwareSyntaxWriter(_ctx);

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be("[StateMachine(\"StateMachine\", typeof(IEnumerable<String>))]");
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithNamedArgument() {
    var sut = AttributeSyntax.Create<StateMachineAttribute>(
      new Dictionary<NamedAttributeParameter, object> { { "Parameter", "StateMachine" } }
    );
    var writer = new TypeAwareSyntaxWriter(_ctx);

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be("[StateMachine(Parameter: \"StateMachine\")]");
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithAllArgumentTypes() {
    var sut = AttributeSyntax.Create<StateMachineAttribute>(
      new Dictionary<NamedAttributeParameter, object> {
        { "NamedParameter", "State" },
        { new NamedAttributeParameter { Name = "NamedField", IsField = true }, "Machine" }
      },
      "StateMachine"
    );
    var writer = new TypeAwareSyntaxWriter(_ctx);

    sut.WriteAsSyntax(writer);

    writer.ToSyntax()
      .Should()
      .Be("[StateMachine(\"StateMachine\", NamedParameter: \"State\", NamedField = \"Machine\")]");
  }

  [Fact]
  public void WriteAttribute_WritesAttributeSyntaxWithNamedFieldArgument() {
    var sut = AttributeSyntax.Create<StateMachineAttribute>(
      new Dictionary<NamedAttributeParameter, object> {
        { new NamedAttributeParameter { Name = "Field", IsField = true }, "StateMachine" }
      }
    );
    var writer = new TypeAwareSyntaxWriter(_ctx);

    sut.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be("[StateMachine(Field = \"StateMachine\")]");
  }

  [Fact]
  public void WriteAttribute_AddsAttributeTypeToNamespaceCache_WhenAttributeHasNamespace() {
    var sut = AttributeSyntax.Create<StateMachineAttribute>();
    var writer = new TypeAwareSyntaxWriter(_ctx);

    sut.WriteAsSyntax(writer);

    _ctx.UsedNamespaces.First().Should().Be("System.Runtime.CompilerServices");
  }
}
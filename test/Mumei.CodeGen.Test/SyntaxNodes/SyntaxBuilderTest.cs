using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using Mumei.Test.SyntaxNodes.Stubs;

namespace Mumei.Test.SyntaxNodes;

public class SyntaxBuilderTest {
  [Fact]
  public void Build_CreatesSyntaxInstance_WithCorrectType() {
    var sut = MakeBuilder();

    var syntax = sut.Build();

    syntax.Should().BeOfType<StubSyntax>();
  }

  [Fact]
  public void Build_CreatesSyntaxInstance_WithIdentifierAndParentSet() {
    var parent = new StubTypeSyntax("");
    var sut = new StubSyntaxBuilder("syntax", parent);

    var syntax = sut.Build();

    syntax.Identifier.Should().Be("syntax");
    syntax.Parent.Should().Be(parent);
  }

  [Fact]
  public void SetVisibility_SetsVisibilityOnSyntax() {
    var sut = MakeBuilder();

    sut.SetVisibility(SyntaxVisibility.Public);
    var syntax = sut.Build();

    syntax.Visibility.Should().Be(SyntaxVisibility.Public);
  }

  [Fact]
  public void AddAttribute_AddsAttributeToField() {
    var sut = MakeBuilder();

    sut.AddAttribute<StateMachineAttribute>();
    var syntax = sut.Build();

    syntax.Attributes.Should().HaveCount(1);
  }

  [Fact]
  public void AddAttribute_AddsAttributeWithArgumentsToSyntax() {
    var sut = MakeBuilder();

    sut.AddAttribute<StateMachineAttribute>("Foo", "Bar");
    var syntax = sut.Build();

    syntax.Attributes.First().Should().BeEquivalentTo(AttributeUsage.Create<StateMachineAttribute>("Foo", "Bar"));
  }

  [Fact]
  public void AddAttribute_AddsAttributeWithNamedArgumentsToSyntax() {
    var sut = MakeBuilder();

    sut.AddAttribute<StateMachineAttribute>(new Dictionary<NamedAttributeParameter, object> {{"", "FooBar"}});
    var syntax = sut.Build();

    syntax.Attributes.First().Should()
          .BeEquivalentTo(AttributeUsage.Create<StateMachineAttribute>(
            new Dictionary<NamedAttributeParameter, object> {{"", "FooBar"}}));
  }

  [Fact]
  public void AddAttribute_AddsAttributeWithNamedAndPositionalArgumentsToSyntax() {
    var sut = MakeBuilder();

    sut.AddAttribute<StateMachineAttribute>(new Dictionary<NamedAttributeParameter, object> {{"", "FooBar"}}, "Foo",
      "Bar");
    var syntax = sut.Build();

    syntax.Attributes.First().Should()
          .BeEquivalentTo(AttributeUsage.Create<StateMachineAttribute>(
            new Dictionary<NamedAttributeParameter, object> {{"", "FooBar"}}, "Foo", "Bar"));
  }

  private SyntaxBuilder<StubSyntax> MakeBuilder() {
    return new StubSyntaxBuilder("");
  }
}
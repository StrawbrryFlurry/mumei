namespace Mumei.Test.SyntaxNodes;
/*
public class TypeSyntaxTest {
  [Fact]
  public void GetAttributeSyntax_ReturnsNull_WhenSyntaxHasNoAttributes() {
    var sut = new TypeSyntaxImpl();

    var syntax = sut.GetAttributeSyntax();

    syntax.Should().BeNull();
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingTheAttributeWithNoNewLine_WhenSyntaxHasOneAttributeAndSameLineIsFalse() {
    var sut = new TypeSyntaxImpl {
      Attributes = new[] {
        AttributeSyntax.Create<StateMachineAttribute>()
      }
    };

    var syntax = sut.GetAttributeSyntax(true);

    syntax.Should().Be("[StateMachine()]");
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingTheAttributeWithNewLineAtTheEnd_WhenSyntaxHasOneAttribute() {
    var sut = new TypeSyntaxImpl {
      Attributes = new[] {
        AttributeSyntax.Create<StateMachineAttribute>()
      }
    };

    var syntax = sut.GetAttributeSyntax();

    syntax.Should().Be(Line("[StateMachine()]"));
  }

  [Fact]
  public void
    GetAttributeSyntax_ReturnsStringRepresentingMultipleAttributesOnMultipleLines_WhenSyntaxHasMultipleAttributes() {
    var sut = new TypeSyntaxImpl {
      Attributes = new[] {
        AttributeSyntax.Create<StateMachineAttribute>(),
        AttributeSyntax.Create<StateMachineAttribute>()
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
    var sut = new TypeSyntaxImpl {
      Attributes = new[] {
        AttributeSyntax.Create<StateMachineAttribute>(),
        AttributeSyntax.Create<StateMachineAttribute>()
      }
    };

    var syntax = sut.WriteAttributes();

    syntax.Should().Be("[StateMachine()] [StateMachine()]");
  }

  [Fact]
  public void SetVisibility_SetsVisibilityOnSyntax() {
    var sut = new TypeSyntaxImpl();

    sut.SetVisibility(SyntaxVisibility.Public);

    sut.Visibility.Should().Be(SyntaxVisibility.Public);
  }

  [Fact]
  public void AddAttribute_AddsAttributeToField() {
    var sut = new TypeSyntaxImpl();

    sut.AddAttribute<StateMachineAttribute>();

    sut.Attributes.Should().HaveCount(1);
  }

  [Fact]
  public void AddAttribute_AddsAttributeWithArgumentsToSyntax() {
    var sut = new TypeSyntaxImpl();

    sut.AddAttribute<StateMachineAttribute>("Foo", "Bar");

    sut.Attributes.First().Should().BeEquivalentTo(AttributeSyntax.Create<StateMachineAttribute>("Foo", "Bar"));
  }

  [Fact]
  public void AddAttribute_AddsAttributeWithNamedArgumentsToSyntax() {
    var sut = new TypeSyntaxImpl();

    sut.AddAttribute<StateMachineAttribute>(new Dictionary<NamedAttributeParameter, object> { { "", "FooBar" } });

    sut.Attributes.First().Should()
      .BeEquivalentTo(AttributeSyntax.Create<StateMachineAttribute>(
        new Dictionary<NamedAttributeParameter, object> { { "", "FooBar" } }));
  }

  [Fact]
  public void AddAttribute_AddsAttributeWithNamedAndPositionalArgumentsToSyntax() {
    var sut = new TypeSyntaxImpl();

    sut.AddAttribute<StateMachineAttribute>(new Dictionary<NamedAttributeParameter, object> { { "", "FooBar" } },
      "Foo",
      "Bar");

    sut.Attributes.First().Should()
      .BeEquivalentTo(AttributeSyntax.Create<StateMachineAttribute>(
        new Dictionary<NamedAttributeParameter, object> { { "", "FooBar" } }, "Foo", "Bar"));
  }

  private class TypeSyntaxImpl : TypeSyntax {
    public TypeSyntaxImpl() : base("") { }

    public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) { }
  }
}
*/
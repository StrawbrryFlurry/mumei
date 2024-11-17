using System.Runtime.CompilerServices;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.SyntaxNodes;

public class AttributeListSyntaxTests {
    private readonly SyntaxTypeContext _ctx = new();

    [Fact]
    public void Clone_ReturnsNewAttributeListWithClonedAttributes() {
        var sut = new AttributeListSyntax();
        sut.AddAttribute<StateMachineAttribute>();

        var clone = sut.Clone<AttributeListSyntax>();

        clone.Parent.Should().BeNull();
        clone.ElementAt(0).Parent.Should().Be(clone);
    }

    [Fact]
    public void AddAttribute_SetsSelfAsParent() {
        var sut = new AttributeListSyntax();

        sut.AddAttribute<StateMachineAttribute>();

        sut.ElementAt(0).Parent.Should().Be(sut);
    }

    [Fact]
    public void AddAttribute_SetsSelfAsParent_WhenOverloadIsPositionalArguments() {
        var sut = new AttributeListSyntax();

        sut.AddAttribute<StateMachineAttribute>(new object());

        sut.ElementAt(0).Parent.Should().Be(sut);
    }

    [Fact]
    public void AddAttribute_SetsSelfAsParent_WhenOverloadIsPositionalAndNamedArguments() {
        var sut = new AttributeListSyntax();

        sut.AddAttribute<StateMachineAttribute>(new Dictionary<NamedAttributeParameter, object>());

        sut.ElementAt(0).Parent.Should().Be(sut);
    }

    [Fact]
    public void WriteAsSyntax_CallsWriteAsSyntaxForAllAttributesInList() {
        var sut = new AttributeListSyntax() {
            new AttributeSyntax(typeof(StateMachineAttribute)),
            new AttributeSyntax(typeof(StateMachineAttribute)),
            new AttributeSyntax(typeof(StateMachineAttribute))
        };
        var writer = new TypeAwareSyntaxWriter(_ctx);

        sut.WriteAsSyntax(writer);

        writer.ToSyntax().Should().Be("""
                                      [StateMachine()][StateMachine()][StateMachine()]
                                      """);
    }

    [Fact]
    public void WriteAsSyntax_WritesEmptyString_WhenListHasNoAttributes() {
        var sut = new AttributeListSyntax();
        var writer = new TypeAwareSyntaxWriter(_ctx);

        sut.WriteAsSyntax(writer);

        writer.ToSyntax().Should().Be("");
    }

    [Fact]
    public void WriteAsSyntax_WritesSingleAttribute_WhenListHasOneAttribute() {
        var sut = new AttributeListSyntax();
        sut.AddAttribute<StateMachineAttribute>();
        var writer = new TypeAwareSyntaxWriter(_ctx);

        sut.WriteAsSyntax(writer);

        writer.ToSyntax().Should().Be("[StateMachine()]");
    }

    [Fact]
    public void WriteAsSyntax_AppliesSeparationStrategy_WhenWritingMultipleAttributes() {
        var sut = new AttributeListSyntax(SeparationStrategy.NewLine);
        sut.AddAttribute<StateMachineAttribute>();
        sut.AddAttribute<StateMachineAttribute>();
        var writer = new TypeAwareSyntaxWriter(_ctx);

        sut.WriteAsSyntax(writer);

        writer.ToSyntax().Should().Be(Line("[StateMachine()]") +
                                      "[StateMachine()]");
    }
}
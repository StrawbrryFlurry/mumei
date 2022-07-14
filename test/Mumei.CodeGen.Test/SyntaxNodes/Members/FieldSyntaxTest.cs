using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;
using static Mumei.Test.Utils.StringExtensions;

namespace Mumei.Test.SyntaxNodes.Members;

public class FieldSyntaxTest {
  private readonly SyntaxTypeContext? _context = new();

  [Fact]
  public void SetInitialValue_SetsInitializerInField() {
    var field = new FieldSyntax("field", null!);

    field.SetInitialValue("FooBar");

    field.Initializer.Should().Be("FooBar");
  }

  [Fact]
  public void WriteAsSyntax_Field() {
    var field = new FieldSyntax("field", null!) {
      Type = typeof(string)
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("String field;"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithSingleAccessModifier() {
    var field = new FieldSyntax("field", null!) {
      Type = typeof(string),
      Visibility = SyntaxVisibility.Public
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("public String field;"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithInitializer() {
    var field = new FieldSyntax("field", null!) {
      Type = typeof(string),
      Initializer = "FooBar",
      Visibility = SyntaxVisibility.Public
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("public String field = \"FooBar\";"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithAttribute() {
    var field = new FieldSyntax("field", null!) {
      Type = typeof(string),
      Initializer = "FooBar",
      Visibility = SyntaxVisibility.Public,
      Attributes = new[]
        {AttributeUsage.Create<StateMachineAttribute>()}
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("[StateMachine()]") +
                                  Line("public String field = \"FooBar\";"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithAttributes() {
    var field = new FieldSyntax("field", null!) {
      Type = typeof(string),
      Initializer = "FooBar",
      Visibility = SyntaxVisibility.Public,
      Attributes = new[] {
        AttributeUsage.Create<StateMachineAttribute>(),
        AttributeUsage.Create<StateMachineAttribute>()
      }
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("[StateMachine()]") +
                                  Line("[StateMachine()]") +
                                  Line("public String field = \"FooBar\";"));
  }
}
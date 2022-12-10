using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.SyntaxNodes.Members;

public class FieldSyntaxTests {
  private readonly SyntaxTypeContext? _context = new();

  [Fact]
  public void SetInitializer_SetsSelfAsParentOfExpression() {
    var field = new FieldSyntax<string>("field", null!);

    field.SetInitialValue("FooBar");

    field.Initializer!.Parent.Should().Be(field);
  }

  [Fact]
  public void SetInitialValue_SetsInitializerValueAsConstantExpression_WhenInitializerIsValue() {
    var field = new FieldSyntax<string>("field", null!);

    field.SetInitialValue("FooBar");

    ((ConstantExpression)field.Initializer!).Value.Should().Be("FooBar");
  }

  [Fact]
  public void SetInitialValue_SetsInitializerValueExpression_WhenInitializerIsExpression() {
    var field = new FieldSyntax<MemberInfo>("field", null!);

    field.SetInitialValue(() => typeof(FieldSyntax).GetField("Initializer")!);

    field.Initializer!.ParseExpressionToSyntaxString(NoopSyntaxWriter.Instance)
      .Should()
      .Be("typeof(FieldSyntax).GetField(\"Initializer\")");
  }

  [Fact]
  public void WriteAsSyntax_Field() {
    var field = new FieldSyntax<string>("field", null!);

    field.WriteSyntaxAsString().Should().Be(Line("String field;"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithSingleAccessModifier() {
    var field = new FieldSyntax<string>("field", null!) {
      Visibility = SyntaxVisibility.Public
    };

    field.WriteSyntaxAsString().Should().Be(Line("public String field;"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithInitializer() {
    var field = new FieldSyntax<string>("field", null!) {
      Initializer = Expression.Constant("FooBar"),
      Visibility = SyntaxVisibility.Public
    };

    field.WriteSyntaxAsString().Should().Be(Line("public String field = \"FooBar\";"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithAttribute() {
    var field = new FieldSyntax<string>("field", null!) {
      Initializer = Expression.Constant("FooBar"),
      Visibility = SyntaxVisibility.Public
    };
    field.AttributeList.AddAttribute<StateMachineAttribute>();

    field.WriteSyntaxAsString().Should().Be(Line("[StateMachine()]") +
                                            Line("public String field = \"FooBar\";"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithAttributes() {
    var field = new FieldSyntax<string>("field", null!) {
      Type = typeof(string),
      Initializer = Expression.Constant("FooBar"),
      Visibility = SyntaxVisibility.Public
    };
    field.AttributeList.AddAttribute<StateMachineAttribute>();
    field.AttributeList.AddAttribute<StateMachineAttribute>();

    field.WriteSyntaxAsString().Should().Be(Line("[StateMachine()]") +
                                            Line("[StateMachine()]") +
                                            Line("public String field = \"FooBar\";"));
  }

  [Fact]
  public void WriteAsSyntax_AddsFieldTypeToSyntaxContext() {
    var field = new FieldSyntax<string>("field", null!) {
      Type = typeof(string)
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("String field;"));
  }

  [Fact]
  public void Clone_ReturnsNewField_WithSameTypeAndIdentifier() {
    var field = new FieldSyntax<string>("field", null!);

    var sut = field.Clone<FieldSyntax<string>>();

    sut.Type.Should().Be<string>();
    sut.Identifier.Should().Be("field");
    sut.Parent.Should().BeNull();
  }

  [Fact]
  public void Clone_ReturnsNewField_WithSameAttributes() {
    var field = new FieldSyntax<string>("field", null!) {
      AttributeList = {
        AttributeSyntax.Create<StateMachineAttribute>(),
        AttributeSyntax.Create<StateMachineAttribute>()
      }
    };

    var sut = field.Clone<FieldSyntax<string>>();

    sut.AttributeList.Should().HaveCount(2);
    sut.AttributeList.Parent.Should().Be(sut);
    sut.AttributeList.All(a => a.Type == typeof(StateMachineAttribute)).Should().BeTrue();
  }
}
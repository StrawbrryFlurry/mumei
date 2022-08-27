﻿using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxNodes.Members;

public class FieldSyntaxTests {
  private readonly SyntaxTypeContext? _context = new();

  [Fact]
  public void SetInitialValue_SetsInitializerInField() {
    var field = new FieldSyntax<string>("field", null!);

    field.SetInitialValue("FooBar");

    ((ConstantExpression)field.Initializer!).Value.Should().Be("FooBar");
  }

  [Fact]
  public void WriteAsSyntax_Field() {
    var field = new FieldSyntax<string>("field", null!) {
      Type = typeof(string)
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("String field;"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithSingleAccessModifier() {
    var field = new FieldSyntax<string>("field", null!) {
      Type = typeof(string),
      Visibility = SyntaxVisibility.Public
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("public String field;"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithInitializer() {
    var field = new FieldSyntax<string>("field", null!) {
      Type = typeof(string),
      Initializer = Expression.Constant("FooBar"),
      Visibility = SyntaxVisibility.Public
    };
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("public String field = \"FooBar\";"));
  }

  [Fact]
  public void WriteAsSyntax_FieldWithAttribute() {
    var field = new FieldSyntax<string>("field", null!) {
      Type = typeof(string),
      Initializer = Expression.Constant("FooBar"),
      Visibility = SyntaxVisibility.Public
    };
    field.AttributeList.AddAttribute<StateMachineAttribute>();
    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("[StateMachine()]") +
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

    var writer = new TypeAwareSyntaxWriter(_context);

    field.WriteAsSyntax(writer);

    writer.ToSyntax().Should().Be(Line("[StateMachine()]") +
                                  Line("[StateMachine()]") +
                                  Line("public String field = \"FooBar\";"));
  }
}
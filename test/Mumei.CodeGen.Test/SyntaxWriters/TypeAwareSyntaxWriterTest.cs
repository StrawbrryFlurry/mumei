using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxWriters;

public class TypeAwareSyntaxWriterTest {
  [Fact]
  public void IncludeTypeNamespace_AddsNamespaceOfTheSpecifiedTypeToContext() {
    var ctx = new SyntaxTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.IncludeTypeNamespace(typeof(string));

    ctx.UsedNamespaces.First().Should().Be(typeof(string).Namespace);
  }

  [Fact]
  public void IncludeTypeNamespace_AddsNamespaceOfGenericArgumentsToContext_WhenTypeIsGeneric() {
    var ctx = new SyntaxTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.IncludeTypeNamespace(typeof(IEnumerable<string>));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(IEnumerable<>).Namespace, typeof(string).Namespace);
  }

  [Fact]
  public void ConvertValueToStringRepresentation_ReturnsStringRepresentationOfTheSpecifiedValue() {
    var ctx = new SyntaxTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.ConvertExpressionValueToSyntax(SyntaxVisibility.Internal)
       .Should()
       .Be($"{nameof(SyntaxVisibility)}.Internal");

    sut.ConvertExpressionValueToSyntax(1).Should().Be("1");
    sut.ConvertExpressionValueToSyntax(1.2).Should().Be("1.2");
    sut.ConvertExpressionValueToSyntax("foo").Should().Be("\"foo\"");
    sut.ConvertExpressionValueToSyntax(true).Should().Be("true");
    sut.ConvertExpressionValueToSyntax(false).Should().Be("false");
    sut.ConvertExpressionValueToSyntax(null).Should().Be("null");

    sut.ConvertExpressionValueToSyntax(SyntaxVisibility.Internal)
       .Should()
       .Be($"{nameof(SyntaxVisibility)}.Internal");
    sut.ConvertExpressionValueToSyntax(typeof(StateMachineAttribute)).Should()
       .Be($"typeof({nameof(StateMachineAttribute)})");
    sut.ConvertExpressionValueToSyntax(typeof(IEnumerable<string>)).Should().Be("typeof(IEnumerable<String>)");
  }

  [Fact]
  public void ConvertValueToStringRepresentation_AddsTypeNamespaceToContext_WhenValueIsType() {
    var ctx = new SyntaxTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.ConvertExpressionValueToSyntax(typeof(string));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(string).Namespace);
  }

  [Fact]
  public void
    ConvertValueToStringRepresentation_AddsTypeGenericArgumentsNamespaceToContext_WhenTypeHasGenericArguments() {
    var ctx = new SyntaxTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.ConvertExpressionValueToSyntax(typeof(IEnumerable<string>));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(IEnumerable<>).Namespace, typeof(string).Namespace);
  }
}
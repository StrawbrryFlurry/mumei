using System.Runtime.CompilerServices;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.SyntaxWriters;

public class TypeAwareSyntaxWriterTests {
  [Fact]
  public void IncludeTypeNamespace_AddsNamespaceOfTheSpecifiedTypeToContext() {
    var ctx = new SyntaxTypeContext();

    ctx.IncludeTypeNamespace(typeof(string));

    ctx.UsedNamespaces.First().Should().Be(typeof(string).Namespace);
  }

  [Fact]
  public void IncludeTypeNamespace_AddsNamespaceOfGenericArgumentsToContext_WhenTypeIsGeneric() {
    var ctx = new SyntaxTypeContext();

    ctx.IncludeTypeNamespace(typeof(IEnumerable<string>));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(IEnumerable<>).Namespace, typeof(string).Namespace);
  }

  [Fact]
  public void ConvertValueToStringRepresentation_ReturnsStringRepresentationOfTheSpecifiedValue() {
    var ctx = new SyntaxTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.GetValueAsExpressionSyntax(SyntaxVisibility.Internal)
      .Should()
      .Be($"{nameof(SyntaxVisibility)}.Internal");

    sut.GetValueAsExpressionSyntax(1).Should().Be("1");
    sut.GetValueAsExpressionSyntax(1.2).Should().Be("1.2");
    sut.GetValueAsExpressionSyntax("foo").Should().Be("\"foo\"");
    sut.GetValueAsExpressionSyntax(true).Should().Be("true");
    sut.GetValueAsExpressionSyntax(false).Should().Be("false");
    sut.GetValueAsExpressionSyntax(null).Should().Be("null");

    sut.GetValueAsExpressionSyntax(SyntaxVisibility.Internal)
      .Should()
      .Be($"{nameof(SyntaxVisibility)}.Internal");
    sut.GetValueAsExpressionSyntax(typeof(StateMachineAttribute)).Should()
      .Be($"typeof({nameof(StateMachineAttribute)})");
    sut.GetValueAsExpressionSyntax(typeof(IEnumerable<string>)).Should().Be("typeof(IEnumerable<String>)");
  }

  [Fact]
  public void ConvertValueToStringRepresentation_AddsTypeNamespaceToContext_WhenValueIsType() {
    var ctx = new SyntaxTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.WriteValueAsExpressionSyntax(typeof(string));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(string).Namespace);
  }

  [Fact]
  public void
    ConvertValueToStringRepresentation_AddsTypeGenericArgumentsNamespaceToContext_WhenTypeHasGenericArguments() {
    var ctx = new SyntaxTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.WriteValueAsExpressionSyntax(typeof(IEnumerable<string>));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(IEnumerable<>).Namespace, typeof(string).Namespace);
  }
}
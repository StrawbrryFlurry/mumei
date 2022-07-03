using System.Runtime.CompilerServices;
using FluentAssertions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.Test.SyntaxWriters;

public class TypeAwareSyntaxWriterTest {
  [Fact]
  public void IncludeTypeNamespace_AddsNamespaceOfTheSpecifiedTypeToContext() {
    var ctx = new WriterTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.IncludeTypeNamespace(typeof(string));

    ctx.UsedNamespaces.First().Should().Be(typeof(string).Namespace);
  }

  [Fact]
  public void IncludeTypeNamespace_AddsNamespaceOfGenericArgumentsToContext_WhenTypeIsGeneric() {
    var ctx = new WriterTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.IncludeTypeNamespace(typeof(IEnumerable<string>));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(IEnumerable<>).Namespace, typeof(string).Namespace);
  }

  [Fact]
  public void ConvertValueToStringRepresentation_ReturnsStringRepresentationOfTheSpecifiedValue() {
    var ctx = new WriterTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.ConvertValueToStringRepresentation(MemberVisibility.Internal).Should().Be("MemberVisibility.Internal");

    sut.ConvertValueToStringRepresentation(1).Should().Be("1");
    sut.ConvertValueToStringRepresentation(1.2).Should().Be("1.2");
    sut.ConvertValueToStringRepresentation("foo").Should().Be("\"foo\"");
    sut.ConvertValueToStringRepresentation(true).Should().Be("true");
    sut.ConvertValueToStringRepresentation(false).Should().Be("false");
    sut.ConvertValueToStringRepresentation(null).Should().Be("null");

    sut.ConvertValueToStringRepresentation(MemberVisibility.Internal).Should().Be("MemberVisibility.Internal");
    sut.ConvertValueToStringRepresentation(typeof(StateMachineAttribute)).Should().Be("typeof(StateMachineAttribute)");
    sut.ConvertValueToStringRepresentation(typeof(IEnumerable<string>)).Should().Be("typeof(IEnumerable<String>)");
  }

  [Fact]
  public void ConvertValueToStringRepresentation_AddsTypeNamespaceToContext_WhenValueIsType() {
    var ctx = new WriterTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.ConvertValueToStringRepresentation(typeof(string));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(string).Namespace);
  }

  [Fact]
  public void
    ConvertValueToStringRepresentation_AddsTypeGenericArgumentsNamespaceToContext_WhenTypeHasGenericArguments() {
    var ctx = new WriterTypeContext();
    var sut = new TypeAwareSyntaxWriter(ctx);

    sut.ConvertValueToStringRepresentation(typeof(IEnumerable<string>));

    ctx.UsedNamespaces.Should().ContainInOrder(typeof(IEnumerable<>).Namespace, typeof(string).Namespace);
  }
}
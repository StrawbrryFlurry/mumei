using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing.Comp;

namespace Mumei.Roslyn.Tests.Reflection;

public sealed class CompilationAttributeTests {
  private const string AttributeDeclaration =
    $$"""
      public class {{nameof(AttributeDeclaration)}}Attribute : System.Attribute { }
      """;

  private const string UsingAttributeDeclaration =
    $$"""
      {{AttributeDeclaration}}

      [{{nameof(AttributeDeclaration)}}]
      public class {{nameof(UsingAttributeDeclaration)}} { }
      """;

  [Fact]
  public void Type_ReturnsBackingCompilationTypeForSymbol() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeDeclaration);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.Type.Should().Be(new RoslynType(attribute.AttributeClass!));
  }

  private const string UsingGlobalAttribute =
    $$"""
      [System.Obsolete]
      public class {{nameof(UsingGlobalAttribute)}} { }
      """;

  [Fact]
  public void Is_ReturnsTrue_WhenAttributeIsOfGivenType() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingGlobalAttribute);

    var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

    actual.Is<ObsoleteAttribute>().Should().BeTrue();
  }

  [Fact]
  public void Is_ReturnsFalse_WhenAttributeIsNotOfGivenType() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingGlobalAttribute);

    var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

    actual.Is<AttributeUsageAttribute>().Should().BeFalse();
  }

  private const string UsingGenericAttribute =
    $$"""
      [{{nameof(GenericAttribute<int>)}}<int>]
      public class {{nameof(UsingGenericAttribute)}} { }
      """;

  [Fact]
  public void IsConstructedGenericTypeOf_ReturnsTrue_WhenAttributeIsConstructedGenericTypeOfGivenType() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(
      UsingGenericAttribute,
      s => s.WithUsing<GenericAttribute<int>>()
    );

    var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

    actual.IsConstructedGenericTypeOf(typeof(GenericAttribute<>)).Should().BeTrue();
  }

  [Fact]
  public void IsConstructedGenericTypeOf_ReturnsFalse_WhenAttributeIsNotConstructedGenericTypeOfGivenType() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(
      UsingGenericAttribute,
      s => s.WithUsing<GenericAttribute<int>>()
    );

    var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

    actual.IsConstructedGenericTypeOf(typeof(GenericAttributeWithTwoArguments<,>)).Should().BeFalse();
  }

  private AttributeData GetNthAttributeFromTypeSymbol(ITypeSymbol typeSymbol, int n = 0) {
    return typeSymbol.GetAttributes()[n];
  }
}

public sealed class GenericAttribute<T> : Attribute { }

public sealed class GenericAttributeWithTwoArguments<T1, T2> : Attribute { }
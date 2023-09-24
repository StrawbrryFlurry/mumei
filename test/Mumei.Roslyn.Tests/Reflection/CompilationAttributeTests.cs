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

  private const string AttributeWithCtorArguments =
    $$"""
      public class {{nameof(AttributeWithCtorArguments)}}Attribute : System.Attribute {
        public {{nameof(AttributeWithCtorArguments)}}Attribute(int value) { }
        public {{nameof(AttributeWithCtorArguments)}}Attribute(int foo, string bar) { }
      }
      """;

  private const string UsingAttributeWithOneCtorArgument =
    $$"""
      {{AttributeWithCtorArguments}}

      [{{nameof(AttributeWithCtorArguments)}}(42)]
      public class {{nameof(UsingAttributeWithOneCtorArgument)}} { }
      """;

  [Fact]
  public void GetArgument_ReturnsNull_WhenAttributeHasNoCtorArguments() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeDeclaration);

    var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

    actual.GetArgument<string?>("", "value").Should().BeNull();
  }

  [Fact]
  public void GetArgument_ReturnsNull_WhenAttributeHasNoCtorArgumentWithMatchingName() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeWithOneCtorArgument);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<int?>("", "foo").Should().BeNull();
  }

  [Fact]
  public void GetArgument_ReturnsCtorArgumentWithMatchingName_WhenAttributeHasOneCtorArgument() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeWithOneCtorArgument);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<int?>("", "value").Should().Be(42);
  }

  private const string UsingAttributeWithTwoCtorArguments =
    $$"""
      {{AttributeWithCtorArguments}}

      [{{nameof(AttributeWithCtorArguments)}}(42, "bar")]
      public class {{nameof(UsingAttributeWithTwoCtorArguments)}} { }
      """;

  [Fact]
  public void GetArgument_ReturnsCtorArgumentWithMatchingName_WhenAttributeHasTwoCtorArgumentsAndIsFirstArgument() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeWithTwoCtorArguments);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<int?>("", "foo").Should().Be(42);
  }

  [Fact]
  public void GetArgument_ReturnsCtorArgumentWithMatchingName_WhenAttributeHasTwoCtorArgumentsIsSecondArgument() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeWithTwoCtorArguments);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<string?>("", "bar").Should().Be("bar");
  }

  private const string AttributeWithProperties =
    $$"""
      public class {{nameof(AttributeWithProperties)}}Attribute : System.Attribute {
        public int Foo { get; set; }
        public string Bar { get; set; }
      }
      """;

  private const string UsingAttributeWithOneProperty =
    $$"""
      {{AttributeWithProperties}}

      [{{nameof(AttributeWithProperties)}}(Foo = 42)]
      public class {{nameof(UsingAttributeWithOneProperty)}} { }
      """;

  [Fact]
  public void GetArgument_ReturnsNull_WhenAttributeHasNoProperties() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeDeclaration);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<int?>("", "value").Should().BeNull();
  }

  [Fact]
  public void GetArgument_ReturnsNull_WhenAttributeHasNoPropertyWithMatchingName() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeWithOneProperty);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<string?>("", "bar").Should().BeNull();
  }

  [Fact]
  public void GetArgument_ReturnsPropertyWithMatchingName_WhenAttributeHasOneProperty() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeWithOneProperty);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<int>("Foo", "").Should().Be(42);
  }

  private const string UsingAttributeWithTwoProperties =
    $$"""
      {{AttributeWithProperties}}

      [{{nameof(AttributeWithProperties)}}(Foo = 42, Bar = "bar")]
      public class {{nameof(UsingAttributeWithTwoProperties)}} { }
      """;

  [Fact]
  public void GetArgument_ReturnsPropertyWithMatchingName_WhenAttributeHasTwoPropertiesAndIsFirstProperty() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeWithTwoProperties);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<int>("Foo", "").Should().Be(42);
  }

  [Fact]
  public void GetArgument_ReturnsPropertyWithMatchingName_WhenAttributeHasTwoPropertiesAndIsSecondProperty() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(UsingAttributeWithTwoProperties);
    var attribute = GetNthAttributeFromTypeSymbol(s);

    var actual = new RoslynAttribute(attribute);

    actual.GetArgument<string>("Bar", "").Should().Be("bar");
  }

  private AttributeData GetNthAttributeFromTypeSymbol(ITypeSymbol typeSymbol, int n = 0) {
    return typeSymbol.GetAttributes()[n];
  }
}

public sealed class GenericAttribute<T> : Attribute { }

public sealed class GenericAttributeWithTwoArguments<T1, T2> : Attribute { }
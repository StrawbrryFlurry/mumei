using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing.Comp;
using Mumei.Roslyn.Testing.Template;

namespace Mumei.Roslyn.Tests.Reflection;

public sealed class CompilationTypeTests {
  private static readonly CompilationType TypeInGlobalNamespace =
    $$"""public class {{nameof(TypeInGlobalNamespace)}} { }""";

  [Fact]
  public void GetFullName_ReturnsTypeName_WhenTypeIsInGlobalNamespace() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeInGlobalNamespace);

    var actual = new RoslynType(s).GetFullName();

    actual.Should().Be(nameof(TypeInGlobalNamespace));
  }

  private static readonly CompilationType TypeInNamespace =
    $$"""
      namespace NamespaceOne;
      public class {{nameof(TypeInNamespace)}} { }
      """;

  [Fact]
  public void GetFullName_ReturnsFullTypeName_WhenTypeIsInNamespace() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeInNamespace);

    var actual = new RoslynType(s).GetFullName();

    actual.Should().Be($"NamespaceOne.{nameof(TypeInNamespace)}");
  }

  private static readonly CompilationType TypeInNestedNamespace =
    $$"""
      namespace NamespaceOne.NamespaceTwo;
      public class {{nameof(TypeInNestedNamespace)}} { }
      """;

  [Fact]
  public void GetFullName_ReturnsFullTypeName_WhenTypeIsInNestedNamespace() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeInNestedNamespace);

    var actual = new RoslynType(s).GetFullName();

    actual.Should().Be($"NamespaceOne.NamespaceTwo.{nameof(TypeInNestedNamespace)}");
  }

  private static readonly CompilationType GenericTypeInGlobalNamespace =
    $$"""
      public class {{nameof(GenericTypeInGlobalNamespace)}}<T> { }
      """;

  [Fact]
  public void GetFullName_ReturnsTypeNameWithGenericParameterCount_WhenTypeIsGenericAndInGlobalNamespace() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(GenericTypeInGlobalNamespace);

    var actual = new RoslynType(s).GetFullName();

    actual.Should().Be($"{nameof(GenericTypeInGlobalNamespace)}`1");
  }

  [Fact]
  public void GetTypeArguments_ReturnsEmptySpan_WhenTypeIsNotGeneric() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeInGlobalNamespace);

    var actual = new RoslynType(s).GetTypeArguments();

    actual.ToArray().Should().BeEmpty();
  }

  private static readonly CompilationType GenericTypeWithOneArgument =
    $$"""
      public class {{nameof(GenericTypeWithOneArgument)}}<T> { }
      """;

  private static readonly CompilationType ConstructedGenericTypeWithOneArgument =
    $$"""
      {{GenericTypeWithOneArgument}}

      public class {{nameof(ConstructedGenericTypeWithOneArgument)}} : {{GenericTypeWithOneArgument:display}}<int> { }
      """;

  [Fact]
  public void GetTypeArguments_ReturnsOneType_WhenTypeIsGenericAndHasOneArgument() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(ConstructedGenericTypeWithOneArgument);

    var actual = new RoslynType(s.BaseType).GetTypeArguments();

    actual.ToArray().Should().HaveCount(1)
      .And.AllBeOfType<RoslynType>()
      .And.Subject.First().GetFullName().Should().Be(typeof(int).FullName);
  }

  [Fact]
  public void GetFirstTypeArgument_ReturnsType_WhenTypeIsGenericAndHasOneArgument() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(ConstructedGenericTypeWithOneArgument);

    var actual = new RoslynType(s.BaseType).GetFirstTypeArgument();

    actual.GetFullName().Should().Be(typeof(int).FullName);
  }

  private static readonly CompilationType GenericTypeWithTwoArguments =
    $$"""
      public class {{nameof(GenericTypeWithTwoArguments)}}<T1, T2> { }
      """;

  private static readonly CompilationType ConstructedGenericTypeWithTwoArguments =
    $$"""
      {{GenericTypeWithTwoArguments}}
      public class {{nameof(ConstructedGenericTypeWithTwoArguments)}} : {{nameof(GenericTypeWithTwoArguments)}}<int, string> { }
      """;

  [Fact]
  public void GetTypeArguments_ReturnsTwoTypes_WhenTypeIsGenericAndHasTwoArguments() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(ConstructedGenericTypeWithTwoArguments);

    var actual = new RoslynType(s.BaseType).GetTypeArguments();

    actual.ToArray().Should().HaveCount(2)
      .And.AllBeOfType<RoslynType>()
      .And.Subject
      .Select(x => x.GetFullName()).Should().ContainInOrder(new[] { typeof(int).FullName, typeof(string).FullName });
  }

  [Fact]
  public void GetAttributes_ReturnsEmptySpan_WhenTypeHasNoAttributes() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeInGlobalNamespace);

    var actual = new RoslynType(s).GetAttributes();

    actual.ToArray().Should().BeEmpty();
  }

  private static readonly CompilationType TypeWithAttribute =
    $$"""
      [{{typeof(AttributeUsageAttribute):display}}(AttributeTargets.Class)]
      public class {{nameof(TypeWithAttribute)}} { }
      """;

  [Fact]
  public void GetAttributes_ReturnsOneAttribute_WhenTypeHasOneAttribute() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeWithAttribute);

    var actual = new RoslynType(s).GetAttributes();

    actual.ToArray().Should().HaveCount(1)
      .And.AllBeOfType<RoslynAttribute>()
      .And.Subject.First().Type.GetFullName().Should().Be(typeof(AttributeUsageAttribute).FullName);
  }

  private static readonly CompilationType GenericAttribute =
    $$"""
      public class {{nameof(GenericAttribute)}}<T> : Attribute { }
      """;

  private static readonly CompilationType TypeWithTwoAttributes =
    $$"""
      [{{typeof(AttributeUsageAttribute):display}}(AttributeTargets.Class)]
      [{{GenericAttribute:display}}<string>]
      public class {{nameof(TypeWithTwoAttributes)}} { }
      """;

  [Fact]
  public void GetAttributes_ReturnsTwoAttributes_WhenTypeHasTwoAttributes() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeWithTwoAttributes);

    var actual = new RoslynType(s).GetAttributes();

    actual.ToArray().Should().HaveCount(2)
      .And.AllBeOfType<RoslynAttribute>()
      .And.Subject.Select(x => x.Type.GetFullName()).Should().ContainInOrder(new[] {
        typeof(AttributeUsageAttribute).FullName,
        $"{nameof(GenericAttribute)}`1"
      });
  }
}
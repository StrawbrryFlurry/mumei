using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing.Comp;

namespace Mumei.Roslyn.Tests.Reflection;

public sealed class CompilationTypeTests {
  private const string TypeInGlobalNamespace = $$"""public class {{nameof(TypeInGlobalNamespace)}} { }""";

  [Fact]
  public void GetFullName_ReturnsTypeName_WhenTypeIsInGlobalNamespace() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeInGlobalNamespace);

    var actual = new RoslynType(s).GetFullName();

    actual.Should().Be(nameof(TypeInGlobalNamespace));
  }

  private const string TypeInNamespace =
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

  private const string TypeInNestedNamespace =
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

  private const string GenericTypeInGlobalNamespace =
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

  private const string GenericTypeWithOneArgument =
    $$"""
      public class {{nameof(GenericTypeWithOneArgument)}}<T> { }
      """;

  private const string ConstructedGenericTypeWithOneArgument =
    $$"""
      {{GenericTypeWithOneArgument}}

      public class {{nameof(ConstructedGenericTypeWithOneArgument)}} : {{nameof(GenericTypeWithOneArgument)}}<int> { }
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

  private const string GenericTypeWithTwoArguments =
    $$"""
      public class {{nameof(GenericTypeWithTwoArguments)}}<T1, T2> { }
      """;

  private const string ConstructedGenericTypeWithTwoArguments =
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

  private const string TypeWithAttribute =
    $$"""
      [AttributeUsage(AttributeTargets.Class)]
      public class {{nameof(TypeWithAttribute)}} { }
      """;

  [Fact]
  public void GetAttributes_ReturnsOneAttribute_WhenTypeHasOneAttribute() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(TypeWithAttribute, s => s.WithUsing<Attribute>());

    var actual = new RoslynType(s).GetAttributes();

    actual.ToArray().Should().HaveCount(1)
      .And.AllBeOfType<RoslynAttribute>()
      .And.Subject.First().Type.GetFullName().Should().Be(typeof(AttributeUsageAttribute).FullName);
  }

  private const string GenericAttribute =
    $$"""
      public class {{nameof(GenericAttribute)}}<T> : Attribute { }
      """;

  private const string TypeWithTwoAttributes =
    $$"""
      {{GenericAttribute}}

      [AttributeUsage(AttributeTargets.Class)]
      [{{nameof(GenericAttribute)}}<string>]
      public class {{nameof(TypeWithTwoAttributes)}} { }
      """;

  [Fact]
  public void GetAttributes_ReturnsTwoAttributes_WhenTypeHasTwoAttributes() {
    var s = TestCompilation.GetSymbolByNameFromSource<ITypeSymbol>(
      TypeWithTwoAttributes, s => s.WithUsing<Attribute>().WithUsing<Attribute>()
    );

    var actual = new RoslynType(s).GetAttributes();

    actual.ToArray().Should().HaveCount(2)
      .And.AllBeOfType<RoslynAttribute>()
      .And.Subject.Select(x => x.Type.GetFullName()).Should().ContainInOrder(new[] {
        typeof(AttributeUsageAttribute).FullName,
        $"{nameof(GenericAttribute)}`1"
      });
  }
}
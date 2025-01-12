using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing.Comp;
using Mumei.Roslyn.Testing.Template;

namespace Mumei.Roslyn.Tests.Reflection;

public sealed class CompilationAttributeTests {
    private static readonly CompilationType SimpleAttribute =
        $$"""
          public class {{nameof(SimpleAttribute)}} : {{typeof(Attribute)}} { }
          """;

    private static readonly CompilationType UsingAttributeDeclaration =
        $$"""
          {{SimpleAttribute}}
          public class {{nameof(UsingAttributeDeclaration)}} { }
          """;

    [Fact]
    public void Type_ReturnsBackingCompilationTypeForSymbol() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeDeclaration);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.Type.Should().Be(new RoslynType(attribute.AttributeClass!));
    }

    private static readonly CompilationType UsingGlobalAttribute =
        $$"""
          {{typeof(ObsoleteAttribute)}}
          public class {{nameof(UsingGlobalAttribute)}} { }
          """;

    [Fact]
    public void Is_ReturnsTrue_WhenAttributeIsOfGivenType() {
        var s = TestCompilation.CompileTypeSymbol(UsingGlobalAttribute);

        var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

        actual.Is<ObsoleteAttribute>().Should().BeTrue();
    }

    [Fact]
    public void Is_ReturnsFalse_WhenAttributeIsNotOfGivenType() {
        var s = TestCompilation.CompileTypeSymbol(UsingGlobalAttribute);

        var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

        actual.Is<AttributeUsageAttribute>().Should().BeFalse();
    }

    private static readonly CompilationType UsingGenericAttribute =
        $$"""
          {{typeof(GenericAttribute<int>)}}
          public class {{nameof(UsingGenericAttribute)}} { }
          """;

    [Fact]
    public void IsConstructedGenericTypeOf_ReturnsTrue_WhenAttributeIsConstructedGenericTypeOfGivenType() {
        var s = TestCompilation.CompileTypeSymbol(UsingGenericAttribute);

        var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

        actual.IsConstructedGenericTypeOf(typeof(GenericAttribute<>)).Should().BeTrue();
    }

    [Fact]
    public void IsConstructedGenericTypeOf_ReturnsFalse_WhenAttributeIsNotConstructedGenericTypeOfGivenType() {
        var s = TestCompilation.CompileTypeSymbol(UsingGenericAttribute);

        var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

        actual.IsConstructedGenericTypeOf(typeof(GenericAttributeWithTwoArguments<,>)).Should().BeFalse();
    }

    private static readonly CompilationType AttributeWithCtorArgumentsAttribute =
        $$"""
          public class {{CompilationType.Name}} : {{typeof(Attribute)}} {
            public {{CompilationType.Name}}(int value) { }
            public {{CompilationType.Name}}(int foo, string bar) { }
          }
          """;

    private static readonly CompilationType UsingAttributeWithOneCtorArgument =
        $$"""
          {{AttributeWithCtorArgumentsAttribute.Call(42):attribute}}
          public class {{CompilationType.Name}} { }
          """;

    [Fact]
    public void GetArgument_ReturnsNull_WhenAttributeHasNoCtorArguments() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeDeclaration);

        var actual = new RoslynAttribute(GetNthAttributeFromTypeSymbol(s));

        actual.GetArgument<string?>("", "value").Should().BeNull();
    }

    [Fact]
    public void GetArgument_ReturnsNull_WhenAttributeHasNoCtorArgumentWithMatchingName() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeWithOneCtorArgument);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.GetArgument<int?>("", "foo").Should().BeNull();
    }

    [Fact]
    public void GetArgument_ReturnsCtorArgumentWithMatchingName_WhenAttributeHasOneCtorArgument() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeWithOneCtorArgument);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.GetArgument<int?>("", "value").Should().Be(42);
    }

    private static readonly CompilationType UsingAttributeWithTwoCtorArguments =
        $$"""
          {{AttributeWithCtorArgumentsAttribute.Call(42, "bar"):attribute}}
          public class {{nameof(UsingAttributeWithTwoCtorArguments)}} { }
          """;

    [Fact]
    public void GetArgument_ReturnsCtorArgumentWithMatchingName_WhenAttributeHasTwoCtorArgumentsAndIsFirstArgument() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeWithTwoCtorArguments);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.GetArgument<int?>("", "foo").Should().Be(42);
    }

    [Fact]
    public void GetArgument_ReturnsCtorArgumentWithMatchingName_WhenAttributeHasTwoCtorArgumentsIsSecondArgument() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeWithTwoCtorArguments);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.GetArgument<string?>("", "bar").Should().Be("bar");
    }

    private static readonly CompilationType AttributeWithPropertiesAttribute =
        $$"""
          public class {{nameof(AttributeWithPropertiesAttribute)}} : {{typeof(Attribute)}} {
            public int Foo { get; set; }
            public string Bar { get; set; }
          }
          """;

    private static readonly CompilationType UsingAttributeWithOneProperty =
        $$"""
          [{{AttributeWithPropertiesAttribute:display}}(Foo = 42)]
          public class {{nameof(UsingAttributeWithOneProperty)}} { }
          """;

    [Fact]
    public void GetArgument_ReturnsNull_WhenAttributeHasNoProperties() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeDeclaration);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.GetArgument<int?>("", "value").Should().BeNull();
    }

    [Fact]
    public void GetArgument_ReturnsNull_WhenAttributeHasNoPropertyWithMatchingName() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeWithOneProperty);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.GetArgument<string?>("", "bar").Should().BeNull();
    }

    [Fact]
    public void GetArgument_ReturnsPropertyWithMatchingName_WhenAttributeHasOneProperty() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeWithOneProperty);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.GetArgument<int>("Foo", "").Should().Be(42);
    }

    private static readonly CompilationType UsingAttributeWithTwoProperties =
        $$"""
          [{{AttributeWithPropertiesAttribute:display}}(Foo = 42, Bar = "bar")]
          public class {{nameof(UsingAttributeWithTwoProperties)}} { }
          """;

    [Fact]
    public void GetArgument_ReturnsPropertyWithMatchingName_WhenAttributeHasTwoPropertiesAndIsFirstProperty() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeWithTwoProperties);
        var attribute = GetNthAttributeFromTypeSymbol(s);

        var actual = new RoslynAttribute(attribute);

        actual.GetArgument<int>("Foo", "").Should().Be(42);
    }

    [Fact]
    public void GetArgument_ReturnsPropertyWithMatchingName_WhenAttributeHasTwoPropertiesAndIsSecondProperty() {
        var s = TestCompilation.CompileTypeSymbol(UsingAttributeWithTwoProperties);
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
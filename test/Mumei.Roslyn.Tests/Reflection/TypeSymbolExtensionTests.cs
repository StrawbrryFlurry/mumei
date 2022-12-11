using Microsoft.CodeAnalysis;
using Mumei.Common;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing;

namespace Mumei.Roslyn.Tests.Reflection;

public sealed class TypeSymbolExtensionTests {
  private const string Source = """
  namespace Test;
  
  public class TestType {
    public int TestField;
    public int TestProperty { get; set; }
    public int TestMethod() => 0;
  }

  public class TestGenericType<T> {
  }
  """;


  private static readonly Compilation Compilation = new TestCompilationBuilder()
    .AddSourceText(Source);

  [Fact]
  public void ToType_ReturnsRuntimeType_WhenTypeExistsAsRuntimeType() {
    var runtimeType = Type.GetType("System.RuntimeType");
    var stringTypeSymbol = Compilation.GetTypeSymbol("System.String");

    var stringType = stringTypeSymbol.ToType();

    stringType.Should().BeOfType(runtimeType);
  }

  [Fact]
  public void ToType_ReturnsRuntimeType_WhenTypeDoesNotExistAsRuntimeType() {
    var reflectionType = CommonModuleAssemblyReference.Assembly.GetType("Mumei.Common.Reflection.ReflectionType");
    var testTypeSymbol = Compilation.GetTypeSymbol("Test.TestType");

    var testType = testTypeSymbol.ToType();

    testType.Should().NotBeNull();
    testType.Should().BeOfType(reflectionType);
  }

  [Fact]
  public void ToType_SetsBasicTypeProperties() {
    var testTypeSymbol = Compilation.GetTypeSymbol("Test.TestType");

    var testType = testTypeSymbol.ToType();

    testType.Name.Should().Be("TestType");
    testType.Namespace.Should().Be("Test");
    testType.FullName.Should().Be("Test.TestType");
    testType.BaseType.Should().Be(typeof(object));

    testType.IsClass.Should().BeTrue();
    testType.IsValueType.Should().BeFalse();
    testType.IsInterface.Should().BeFalse();
    testType.IsEnum.Should().BeFalse();
    testType.IsPrimitive.Should().BeFalse();
    testType.IsArray.Should().BeFalse();
    testType.IsGenericType.Should().BeFalse();
    testType.IsGenericTypeDefinition.Should().BeFalse();
    testType.IsConstructedGenericType.Should().BeFalse();
    testType.IsAbstract.Should().BeFalse();
    testType.IsSealed.Should().BeFalse();
    testType.IsNested.Should().BeFalse();
  }
}
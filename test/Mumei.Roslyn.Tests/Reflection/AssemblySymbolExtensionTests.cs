using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing;

namespace Mumei.Roslyn.Tests.Reflection;

public sealed class AssemblySymbolExtensionTests {
  private const string AssemblyName = "TestAssembly";

  private const string Source = """
  namespace TestNamespace;
  public class TestType {  }
  """;

  private static readonly Compilation Compilation = new TestCompilationBuilder()
    .WithAssemblyName(AssemblyName)
    .AddSourceText(Source);

  [Fact]
  public void ToAssembly_ReturnsInstanceOfAssemblyWithCorrectFullName() {
    var runtimeAssembly = GetAssemblyFromTestType();
    runtimeAssembly.FullName.Should().Be($"{AssemblyName}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
  }

  private Assembly GetAssemblyFromTestType() {
    return Compilation.GetTypeSymbol("TestNamespace.TestType").ContainingAssembly.ToAssembly();
  }
}
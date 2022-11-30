using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Testing;

namespace Mumei.Roslyn.Tests; 

public sealed class SymbolExtensionTests {
  private const string TestSource = """
  using System.Collections.Generic;
  
  class TestClass {
    public string TestField;
    public List<string> TestGenericField;
  }
  """;

  private static readonly Compilation TestCompilation = new TestCompilationBuilder()
    .AddSourceText(TestSource);
  
  [Fact]
  public void GetFullName_ReturnsNameOfTheType_WhenTypeIsInGlobalNamespace() {
    var globalTypeSymbol = TestCompilation.GetTypeSymbol("TestClass");

    globalTypeSymbol.GetFullName().Should().Be("TestClass");
  }
  
  [Fact]
  public void GetFullName_ReturnsNameOfTheTypeIncludingNamespace_WhenTypeIsContainedInOneNamespace() {
    var typeInSingleNamespace = TestCompilation.GetTypeMemberSymbol<IFieldSymbol>("TestClass", "TestField").Type;

    typeInSingleNamespace.GetFullName().Should().Be("System.String");
  }
  
  [Fact]
  public void GetFullName_ReturnsNameOfTheTypeIncludingNamespaces_WhenTypeIsContainedInMultipleNamespaces() {
    var typeInMultipleNamespaces = TestCompilation.GetTypeMemberSymbol<IFieldSymbol>("TestClass", "TestGenericField").Type;

    typeInMultipleNamespaces.GetFullName().Should().Be("System.Collections.Generic.List");
  }
}
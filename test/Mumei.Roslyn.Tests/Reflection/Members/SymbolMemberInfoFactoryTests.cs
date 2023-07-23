using Microsoft.CodeAnalysis;
using Mumei.Common;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing;
using Mumei.Roslyn.Testing.Comp;

namespace Mumei.Roslyn.Tests.Reflection.Members;

public sealed class SymbolMemberInfoFactoryTests {
  private const string Source = """
                                namespace Test;

                                public class TestType {
                                  public int TestField;
                                  public int TestProperty { get; set; }
                                  public int TestMethod() => 0;
                                
                                  public event EventHandler TestEvent;
                                }
                                """;

  private static readonly Compilation Compilation = new TestCompilationBuilder()
    .AddSource(Source);

  public SymbolMemberInfoFactoryTests() {
    var typeSymbol = Compilation.GetTypeByMetadataName("Test.TestType")!;
    DeclaringType = typeSymbol.ToType();
  }

  private Type DeclaringType { get; }

  [Fact]
  public void CreateMemberInfo_CreatesReflectionFieldInfo_WhenMemberSymbolIsFieldSymbol() {
    var fieldSymbol = Compilation.GetTypeMemberSymbol<IFieldSymbol>("Test.TestType", "TestField");
    var sut = new SymbolMemberInfoFactory(fieldSymbol);

    var memberInfo = sut.CreateMemberInfo(DeclaringType);

    memberInfo.Should().BeAssignableTo<ReflectionFieldInfo>();
  }

  [Fact]
  public void CreateMemberInfo_CreatesReflectionPropertyInfo_WhenMemberSymbolIsPropertySymbol() {
    var propertySymbol = Compilation.GetTypeMemberSymbol<IPropertySymbol>("Test.TestType", "TestProperty");
    var sut = new SymbolMemberInfoFactory(propertySymbol);

    var memberInfo = sut.CreateMemberInfo(DeclaringType);

    memberInfo.Should().BeAssignableTo<ReflectionPropertyInfo>();
  }

  [Fact]
  public void CreateMemberInfo_CreatesReflectionMethodInfo_WhenMemberSymbolIsMethodSymbol() {
    var methodSymbol = Compilation.GetTypeMemberSymbol<IMethodSymbol>("Test.TestType", "TestMethod");
    var sut = new SymbolMemberInfoFactory(methodSymbol);

    var memberInfo = sut.CreateMemberInfo(DeclaringType);

    memberInfo.Should().BeOfType<ReflectionMethodInfo>();
  }

  [Fact]
  public void Get_MemberInfoName_ReturnsNameOfMemberSymbol() {
    var fieldSymbol = Compilation.GetTypeMemberSymbol<IFieldSymbol>("Test.TestType", "TestField");
    var sut = new SymbolMemberInfoFactory(fieldSymbol);

    sut.MemberInfoName.Should().Be("TestField");
  }

  [Fact]
  public void Ctor_ThrowsNotSupportedException_WhenMemberSymbolIsEventSymbol() {
    var eventSymbol = Compilation.GetTypeMemberSymbol<IEventSymbol>("Test.TestType", "TestEvent");
    var createFactoryAction = () => new SymbolMemberInfoFactory(eventSymbol);

    createFactoryAction.Should().Throw<NotSupportedException>();
  }

  [Fact]
  public void Ctor_ThrowsNotSupportedException_WhenMemberSymbolIsNotSupported() {
    var unsupportedSymbol = Compilation.GetTypeByMetadataName("Test.TestType")!;
    var createFactoryAction = () => new SymbolMemberInfoFactory(unsupportedSymbol);

    createFactoryAction.Should().Throw<NotSupportedException>();
  }
}
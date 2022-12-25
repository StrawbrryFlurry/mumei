using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing;

namespace Mumei.Roslyn.Tests.Reflection.Members;

public sealed class FieldSymbolExtensionTests : MemberSymbolExtensionTest {
  private const string Source = """
  using System;
  
  namespace Test;
  
  public class TestType : TestBaseType {
    public int PublicField;
    private int PrivateField;
    protected int ProtectedField;
    internal int InternalField;
    protected internal int ProtectedInternalField;
    public static int PublicStaticField;
    public readonly int PublicReadonlyField;
    public const int PublicConstField = 1;

    // FieldInfo does not have abstract, virtual, override, new, sealed, extern, volatile, or unsafe modifiers
    public abstract int PublicReadonlyField;

    [Obsolete]
    public int FieldWithAttribute;

    public int FieldWithoutAttribute;

    public new int BaseTypeHasAttribute;
  }

  public class TestBaseType {
    [Obsolete]
    public int BaseTypeHasAttribute;
  }
  """;

  protected override TestCompilationBuilder CompilationBuilder { get; } = new TestCompilationBuilder()
    .AddSourceText(Source);

  #region Construction

  [Fact]
  public void ToFieldInfoFactory_ReturnsFieldInfoFactory() {
    var fieldSymbol = GetFieldSymbol();

    var sut = fieldSymbol.ToFieldInfoFactory();

    sut.Should().BeAssignableTo<IFieldInfoFactory>();
  }

  [Fact]
  public void CreateFieldInfo_ReturnsFieldInfoInstance() {
    var fieldSymbol = GetFieldSymbol();

    var sut = fieldSymbol.ToFieldInfo(GetDeclaringType());

    sut.Should().BeAssignableTo<FieldInfo>();
  }

  #endregion

  #region Basic Field Information

  [Fact]
  public void CreateFieldInfo_ReturnsFieldInfoWithBasicInformation() {
    var sut = GetFieldInfoWithoutCreatingType("PublicField", out var fieldSymbol);

    sut.Name.Should().Be(fieldSymbol.Name);
    sut.MemberType.Should().Be(MemberTypes.Field);
    sut.DeclaringType.Should().Be(GetDeclaringType());
    sut.FieldType.Should().Be(fieldSymbol.Type.ToType());
  }

  [Fact]
  public void CreateFieldInfo_ReturnsFieldInfoWithCorrectAttributes() {
    var fieldInfo = GetFieldInfoWithoutCreatingType("PublicField", out _);
    fieldInfo.IsPublic.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetFieldInfoWithoutCreatingType("PrivateField", out _);
    fieldInfo.IsPrivate.Should().BeTrue();

    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetFieldInfoWithoutCreatingType("ProtectedField", out _);
    fieldInfo.IsFamily.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetFieldInfoWithoutCreatingType("InternalField", out _);
    fieldInfo.IsAssembly.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetFieldInfoWithoutCreatingType("ProtectedInternalField", out _);
    fieldInfo.IsFamilyOrAssembly.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetFieldInfoWithoutCreatingType("PublicStaticField", out _);
    fieldInfo.IsStatic.Should().BeTrue();
    fieldInfo.IsPublic.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetFieldInfoWithoutCreatingType("PublicReadonlyField", out _);
    fieldInfo.IsPublic.Should().BeTrue();
    fieldInfo.IsInitOnly.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetFieldInfoWithoutCreatingType("PublicConstField", out _);
    fieldInfo.IsLiteral.Should().BeTrue();
    fieldInfo.IsPublic.Should().BeTrue();
    fieldInfo.IsStatic.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
  }

  [Fact]
  public void CreateFieldInfo_ReturnsFieldInfoInstanceWithCustomAttributeData() {
    var fieldInfo = GetFieldInfoWithoutCreatingType("PublicField", out _);
    fieldInfo.CustomAttributes.Should().BeEmpty();

    fieldInfo = GetFieldInfoWithoutCreatingType("FieldWithAttribute", out _);

    var customAttributeData = fieldInfo.CustomAttributes.Single();
    customAttributeData.AttributeType.Should().Be(typeof(ObsoleteAttribute));
  }

  #endregion

  #region Attributes

  [Fact]
  public void IsDefined_ReturnsTrue_WhenAttributeIsDefinedInDerivedTypeAndInheritIsFalse() {
    var fieldInfo = GetFieldInfo("FieldWithAttribute", out _);

    var result = fieldInfo.IsDefined(typeof(ObsoleteAttribute), false);

    result.Should().BeTrue();
  }

  [Fact]
  public void IsDefined_ReturnsFalse_WhenAttributeIsNotDefinedInDerivedTypeAndInheritIsFalse() {
    var fieldInfo = GetFieldInfo("FieldWithoutAttribute", out _);

    var result = fieldInfo.IsDefined(typeof(ObsoleteAttribute), false);

    result.Should().BeFalse();
  }

  // Fields are not inherited
  [Fact]
  public void IsDefined_ReturnsFalse_WhenAttributeIsDefinedInBaseTypeAndInheritIsTrue() {
    var fieldInfo = GetFieldInfo("BaseTypeHasAttribute", out _);

    var result = fieldInfo.IsDefined(typeof(ObsoleteAttribute), true);

    result.Should().BeFalse();
  }

  [Fact]
  public void IsDefined_ReturnsFalse_WhenAttributeIsDefinedInBaseTypeAndInheritIsFalse() {
    var fieldInfo = GetFieldInfo("BaseTypeHasAttribute", out _);

    var result = fieldInfo.IsDefined(typeof(ObsoleteAttribute), false);

    result.Should().BeFalse();
  }

  #endregion

  #region Utilities

  private FieldInfo GetFieldInfoWithoutCreatingType(string fieldName, out IFieldSymbol fieldSymbol) {
    fieldSymbol = GetFieldSymbol(fieldName);
    return fieldSymbol.ToFieldInfo(GetDeclaringType());
  }

  private FieldInfo GetFieldInfo(string fieldName, out IFieldSymbol fieldSymbol) {
    fieldSymbol = GetFieldSymbol(fieldName);
    var declaringType = Compilation.GetTypeSymbol("Test.TestType").ToType();
    return fieldSymbol.ToFieldInfo(declaringType);
  }

  private IFieldSymbol GetFieldSymbol(string name = "PublicField") {
    return Compilation.GetTypeMemberSymbol<IFieldSymbol>("Test.TestType", name);
  }

  #endregion
}
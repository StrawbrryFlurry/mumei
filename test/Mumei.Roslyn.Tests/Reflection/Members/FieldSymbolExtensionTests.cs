using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;
using Mumei.Roslyn.Reflection;
using Mumei.Roslyn.Testing;

namespace Mumei.Roslyn.Tests.Reflection.Members;

public sealed class FieldSymbolExtensionTests : MemberSymbolExtensionTest<IFieldSymbol, FieldInfo> {
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
                                  public const int PublicConstField = 0;
                                
                                  // FieldInfo does not have abstract, virtual, override, new, sealed, extern, volatile, or unsafe modifiers
                                  public abstract int PublicReadonlyField;
                                
                                  [Obsolete("Test", DiagnosticId = "TestId")]
                                  public int FieldWithAttribute;
                                  
                                  [Obsolete("Test")]
                                  [NonSerializedAttribute]
                                  public int FieldWithTwoAttributes;
                                
                                  public int FieldWithoutAttribute;
                                
                                  public new int BaseTypeHasAttribute;
                                }

                                public class TestBaseType {
                                  [Obsolete]
                                  public int BaseTypeHasAttribute;
                                }
                                """;

  protected override TestCompilationBuilder CompilationBuilder { get; } = new TestCompilationBuilder()
    .AddTypeReference<XmlAttribute>()
    .AddSource(Source);

  protected override FieldInfo CreateMemberInfo(IFieldSymbol symbol, Type declaringType) {
    return symbol.ToFieldInfo(declaringType);
  }

  #region Construction

  [Fact]
  public void ToFieldInfoFactory_ReturnsFieldInfoFactory() {
    var fieldSymbol = GetMemberSymbol("PublicField");

    var sut = fieldSymbol.ToFieldInfoFactory();

    sut.Should().BeAssignableTo<IFieldInfoFactory>();
  }

  [Fact]
  public void CreateFieldInfo_ReturnsFieldInfoInstance() {
    var fieldSymbol = GetMemberSymbol("PublicField");

    var sut = fieldSymbol.ToFieldInfo(GetEmptyDeclaringType());

    sut.Should().BeAssignableTo<FieldInfo>();
  }

  #endregion

  #region Basic Field Information

  [Fact]
  public void CreateFieldInfo_ReturnsFieldInfoWithBasicInformation() {
    var sut = GetMemberInfoWithoutCreatingType("PublicField", out var fieldSymbol);

    sut.Name.Should().Be(fieldSymbol.Name);
    sut.MemberType.Should().Be(MemberTypes.Field);
    sut.DeclaringType.Should().Be(GetEmptyDeclaringType());
    sut.FieldType.Should().Be(fieldSymbol.Type.ToType());
  }

  [Fact]
  public void CreateFieldInfo_ReturnsFieldInfoWithCorrectAttributes() {
    var fieldInfo = GetMemberInfoWithoutCreatingType("PublicField", out _);
    fieldInfo.IsPublic.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetMemberInfoWithoutCreatingType("PrivateField", out _);
    fieldInfo.IsPrivate.Should().BeTrue();

    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetMemberInfoWithoutCreatingType("ProtectedField", out _);
    fieldInfo.IsFamily.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetMemberInfoWithoutCreatingType("InternalField", out _);
    fieldInfo.IsAssembly.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetMemberInfoWithoutCreatingType("ProtectedInternalField", out _);
    fieldInfo.IsFamilyOrAssembly.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetMemberInfoWithoutCreatingType("PublicStaticField", out _);
    fieldInfo.IsStatic.Should().BeTrue();
    fieldInfo.IsPublic.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsInitOnly.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetMemberInfoWithoutCreatingType("PublicReadonlyField", out _);
    fieldInfo.IsPublic.Should().BeTrue();
    fieldInfo.IsInitOnly.Should().BeTrue();

    fieldInfo.IsPrivate.Should().BeFalse();
    fieldInfo.IsFamily.Should().BeFalse();
    fieldInfo.IsAssembly.Should().BeFalse();
    fieldInfo.IsFamilyOrAssembly.Should().BeFalse();
    fieldInfo.IsFamilyAndAssembly.Should().BeFalse();
    fieldInfo.IsStatic.Should().BeFalse();
    fieldInfo.IsLiteral.Should().BeFalse();

    fieldInfo = GetMemberInfoWithoutCreatingType("PublicConstField", out _);
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
    var fieldInfo = GetMemberInfoWithoutCreatingType("FieldWithoutAttribute", out _);
    fieldInfo.CustomAttributes.Should().BeEmpty();

    fieldInfo = GetMemberInfoWithoutCreatingType("FieldWithAttribute", out _);

    var customAttributeData = fieldInfo.CustomAttributes.Single();
    customAttributeData.AttributeType.Should().Be(typeof(ObsoleteAttribute));
  }

  #endregion

  #region Attributes

  [Fact]
  public void GetCustomAttributesData_ReturnsCollectionOfCustomAttributesDeclaredOnField_WhenFieldHasNoAttribute() {
    var fieldInfo = GetMemberInfo("FieldWithoutAttribute", out _);

    var attributes = fieldInfo.GetCustomAttributesData().ToArray();

    attributes.Should().BeEmpty();
  }

  [Fact]
  public void GetCustomAttributesData_ReturnsCollectionOfCustomAttributesDeclaredOnField_WhenFieldHasOneAttribute() {
    var fieldInfo = GetMemberInfo("FieldWithAttribute", out _);

    var attributes = fieldInfo.GetCustomAttributesData().ToArray();

    attributes.Should().HaveCount(1);

    var attribute = attributes.Single();
    attribute.AttributeType.Should().Be(typeof(ObsoleteAttribute));
    attribute.ConstructorArguments.Should().ContainSingle(x => x.Value is string && (string)x.Value == "Test");
    attribute.NamedArguments.Should().ContainSingle(x =>
      x.MemberName == "DiagnosticId"
      && x.TypedValue.Value is string
      && (string)x.TypedValue.Value == "TestId"
    );
  }

  [Fact]
  public void GetCustomAttributesData_ReturnsCollectionOfCustomAttributesDeclaredOnField_WhenFieldHasTwoAttributes() {
    var fieldInfo = GetMemberInfo("FieldWithTwoAttributes", out _);

    var attributes = fieldInfo.GetCustomAttributesData().ToArray();

    attributes.Should().HaveCount(2);
    attributes.Should().Contain(a => a.AttributeType == typeof(ObsoleteAttribute));
    attributes.Should().Contain(a => a.AttributeType == typeof(NonSerializedAttribute));
  }

  [Fact]
  public void IsDefined_ReturnsTrue_WhenAttributeIsDefinedInDerivedTypeAndInheritIsFalse() {
    var fieldInfo = GetMemberInfo("FieldWithAttribute", out _);

    var result = fieldInfo.IsDefined(typeof(ObsoleteAttribute), false);

    result.Should().BeTrue();
  }

  [Fact]
  public void IsDefined_ReturnsFalse_WhenAttributeIsNotDefinedInDerivedTypeAndInheritIsFalse() {
    var fieldInfo = GetMemberInfo("FieldWithoutAttribute", out _);

    var result = fieldInfo.IsDefined(typeof(ObsoleteAttribute), false);

    result.Should().BeFalse();
  }

  [Fact]
  public void IsDefined_ReturnsFalse_WhenAttributeIsDefinedInBaseTypeAndInheritIsTrue() {
    var fieldInfo = GetMemberInfo("BaseTypeHasAttribute", out _);

    var result = fieldInfo.IsDefined(typeof(ObsoleteAttribute), true);

    result.Should().BeFalse("Fields are not inherited");
  }

  [Fact]
  public void IsDefined_ReturnsFalse_WhenAttributeIsDefinedInBaseTypeAndInheritIsFalse() {
    var fieldInfo = GetMemberInfo("BaseTypeHasAttribute", out _);

    var result = fieldInfo.IsDefined(typeof(ObsoleteAttribute), false);

    result.Should().BeFalse();
  }

  [Fact]
  public void GetCustomAttributes_bool_ReturnsEmptyCollection_WhenFieldHasNoAttributes() {
    var fieldInfo = GetMemberInfo("FieldWithoutAttribute", out _);

    var attributes = fieldInfo.GetCustomAttributes(false);

    attributes.Should().BeEmpty();
  }

  [Fact]
  public void GetCustomAttributes_bool_ReturnsCollectionOfCustomAttributesDeclaredOnField_WhenFieldHasOneAttribute() {
    var fieldInfo = GetMemberInfo("FieldWithAttribute", out _);

    var attributes = fieldInfo.GetCustomAttributes(false);

    attributes.Should().HaveCount(1);
    attributes.Should().ContainSingle(a => a is ObsoleteAttribute);
  }

  [Fact]
  public void GetCustomAttributes_bool_ReturnsCollectionOfCustomAttributesDeclaredOnField_WhenFieldHasTwoAttributes() {
    var fieldInfo = GetMemberInfo("FieldWithTwoAttributes", out _);

    var attributes = fieldInfo.GetCustomAttributes(false);

    attributes.Should().HaveCount(2);
    attributes.Should().Contain(a => a is ObsoleteAttribute);
    attributes.Should().Contain(a => a is NonSerializedAttribute);
  }

  [Fact]
  public void GetCustomAttributes_Type_bool_ReturnsEmptyCollection_WhenFieldDoesNotHaveAttributeOfType() {
    var fieldInfo = GetMemberInfo("FieldWithAttribute", out _);

    var attributes = fieldInfo.GetCustomAttributes(typeof(StateMachineAttribute), false);

    attributes.Should().BeEmpty();
  }

  [Fact]
  public void GetCustomAttributes_Type_bool_ReturnsSingleAttributeMatchingType_WhenFieldHasAttributeOfType() {
    var fieldInfo = GetMemberInfo("FieldWithAttribute", out _);

    var attributes = fieldInfo.GetCustomAttributes(typeof(ObsoleteAttribute), false);

    attributes.Should().HaveCount(1);
    attributes.Should().Contain(a => a is ObsoleteAttribute);
  }

  #endregion
}
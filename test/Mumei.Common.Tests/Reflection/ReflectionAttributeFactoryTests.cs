using System.Linq.Expressions;
using System.Reflection;
using Mumei.Common.Reflection;
using Mumei.Common.Tests.Extensions;

namespace Mumei.Common.Tests.Reflection;

public sealed class ReflectionAttributeFactoryTests {
  [Fact]
  public void CreateInstance_ThrowsNotSupportedException_WhenAttributeIsNotRuntimeType() {
    var type = ReflectionType.Create(
      "",
      "",
      typeof(Attribute),
      Type.EmptyTypes,
      Type.EmptyTypes,
      false,
      TypeAttributes.Class,
      Array.Empty<IMethodInfoFactory>(),
      Array.Empty<IFieldInfoFactory>(),
      Array.Empty<IPropertyInfoFactory>(),
      ReflectionModule.Create("", Assembly.GetExecutingAssembly(), Type.EmptyTypes)
    );
    var attributeData = CreateCustomAttributeData(type);
    var action = () => ReflectionAttributeFactory.CreateInstance(attributeData);

    action.Should().Throw<NotSupportedException>();
  }

  [Fact]
  public void CreateInstance_ReturnsInstanceOfAttribute_WhenAttributeHasNoCtorParameters() {
    var attributeData = CreateCustomAttributeData<NoCtorParametersAttribute>();
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData);

    attribute.Should().BeOfType<NoCtorParametersAttribute>();
  }

  [Fact]
  public void
    CreateInstance_ThrowsMissingMethodExceptionException_WhenAttributeHasSingleCtorParameterAndBadTypeIsProvided() {
    var attributeData = CreateCustomAttributeData<NoCtorParametersAttribute>(new[] { "test" });

    Action action = () => ReflectionAttributeFactory.CreateInstance(attributeData);

    action.Should().Throw<MissingMethodException>();
  }

  [Fact]
  public void CreateInstance_AssignsNamedParametersAsProperties_WhenAttributeHasSinglePropertyWithName() {
    var attributeData = CreateCustomAttributeData<NoCtorParametersAttribute>(
      Array.Empty<object?>(),
      new[] {
        CreateNamedArgument<NoCtorParametersAttribute>(x => x.NamedValue, 5)
      });
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as NoCtorParametersAttribute;

    attribute.Should().BeOfType<NoCtorParametersAttribute>();
    attribute.NamedValue.Should().Be(5);
  }

  [Fact]
  public void CreateInstance_AssignsNamedParametersAsProperties_WhenAttributeHasSinglePropertyWithNameAndValueIsNull() {
    var attributeData = CreateCustomAttributeData<NoCtorParametersAttribute>(
      Array.Empty<object?>(),
      new[] {
        CreateNamedArgument<NoCtorParametersAttribute>(x => x.NamedValue,
          new CustomAttributeTypedArgument(typeof(string), null))
      });
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as NoCtorParametersAttribute;

    attribute.Should().BeOfType<NoCtorParametersAttribute>();
    attribute.NamedValue.Should().BeNull();
  }

  [Fact]
  public void CreateInstance_AssignsNamedParametersAsField_WhenMemberTargetIsPublicField() {
    var attributeData = CreateCustomAttributeData<NoCtorParametersAttribute>(
      Array.Empty<object?>(),
      new[] {
        CreateNamedArgument<NoCtorParametersAttribute>(x => x.FieldValue, 5)
      });
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as NoCtorParametersAttribute;

    attribute.Should().BeOfType<NoCtorParametersAttribute>();
    attribute.FieldValue.Should().Be(5);
  }

  [Fact]
  public void CreateInstance_ThrowsNotSupportedException_WhenTargetIsNotFieldOrPropertyMember() {
    var attributeData = CreateCustomAttributeData<NoCtorParametersAttribute>(
      Array.Empty<object?>(),
      new[] {
        CreateNamedArgument<NoCtorParametersAttribute>(x => x.MethodTarget, 5)
      });

    var action = () => ReflectionAttributeFactory.CreateInstance(attributeData) as NoCtorParametersAttribute;

    action.Should().Throw<NotSupportedException>();
  }

  [Fact]
  public void CreateInstance_ReturnsInstanceOfAttribute_WhenAttributeHasSingleCtorParameter() {
    var attributeData = CreateCustomAttributeData<WithSingleCtorParametersAttribute>(new object[] { 1 });
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as WithSingleCtorParametersAttribute;

    attribute.Should().BeOfType<WithSingleCtorParametersAttribute>();
    attribute.Value.Should().Be(1);
  }

  [Fact]
  public void
    CreateInstance_ThrowsMissingMethodExceptionException_WhenAttributeHasMultipleCtorParametersAndNoArgumentsAreProvided() {
    var attributeData = CreateCustomAttributeData<WithMultipleCtorParametersAttribute>(Array.Empty<object>());

    Action action = () => ReflectionAttributeFactory.CreateInstance(attributeData);

    action.Should().Throw<MissingMethodException>();
  }

  [Fact]
  public void
    CreateInstance_ThrowsMissingMethodExceptionException_WhenAttributeHasMultipleCtorParametersAndBadOptionalArgumentIsProvided() {
    var attributeData = CreateCustomAttributeData<WithMultipleCtorParametersAttribute>(new object[] { 1, false });

    Action action = () => ReflectionAttributeFactory.CreateInstance(attributeData);

    action.Should().Throw<MissingMethodException>();
  }

  [Fact]
  public void
    CreateInstance_ThrowsMissingMethodExceptionException_WhenAttributeHasMultipleCtorParametersAndNullOptionalArgumentIsProvided() {
    var attributeData = CreateCustomAttributeData(typeof(WithMultipleCtorParametersAttribute), new[] {
      new CustomAttributeTypedArgument(typeof(int), 1),
      new CustomAttributeTypedArgument(typeof(string), null!)
    });

    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as WithMultipleCtorParametersAttribute;

    attribute.Should().BeOfType<WithMultipleCtorParametersAttribute>();
    attribute.Value.Should().Be(1);
    attribute.OptionalValue.Should().BeNull();
  }

  [Fact]
  public void
    CreateInstance_ReturnsInstanceOfAttribute_WhenAttributeHasMultipleCtorParameters_AndOnlyRequiredArgumentsAreProvided() {
    var attributeData = CreateCustomAttributeData<WithMultipleCtorParametersAttribute>(new object[] { 1 });
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as WithMultipleCtorParametersAttribute;

    attribute.Should().BeOfType<WithMultipleCtorParametersAttribute>();
    attribute.Value.Should().Be(1);
    attribute.OptionalValue.Should().BeNull();
  }

  [Fact]
  public void
    CreateInstance_ReturnsInstanceOfAttribute_WhenAttributeHasMultipleCtorParameters_AndAllArgumentsAreProvided() {
    var attributeData = CreateCustomAttributeData<WithMultipleCtorParametersAttribute>(new object[] { 1, "test" });
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as WithMultipleCtorParametersAttribute;

    attribute.Should().BeOfType<WithMultipleCtorParametersAttribute>();
    attribute.Value.Should().Be(1);
    attribute.OptionalValue.Should().Be("test");
  }

  [Fact]
  public void CreateInstance_AssignsNamedParametersAsProperties_WhenAttributeHasMultiplePropertiesAndOneIsAssigned() {
    var attributeData = CreateCustomAttributeData<WithMultipleCtorParametersAttribute>(
      new object[] { 1 },
      new[] {
        CreateNamedArgument<WithMultipleCtorParametersAttribute>(x => x.NamedValue, 5)
      });
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as WithMultipleCtorParametersAttribute;

    attribute.Should().BeOfType<WithMultipleCtorParametersAttribute>();
    attribute.NamedValue.Should().Be(5);
    attribute.NamedOptionalValue.Should().BeNull();
  }

  [Fact]
  public void CreateInstance_AssignsNamedParametersAsProperties_WhenAttributeHasMultiplePropertiesAndAllAreAssigned() {
    var attributeData = CreateCustomAttributeData<WithMultipleCtorParametersAttribute>(
      new object[] { 1 },
      new[] {
        CreateNamedArgument<WithMultipleCtorParametersAttribute>(x => x.NamedValue, 5),
        CreateNamedArgument<WithMultipleCtorParametersAttribute>(x => x.NamedOptionalValue, "foo")
      });
    var attribute = ReflectionAttributeFactory.CreateInstance(attributeData) as WithMultipleCtorParametersAttribute;

    attribute.Should().BeOfType<WithMultipleCtorParametersAttribute>();
    attribute.NamedValue.Should().Be(5);
    attribute.NamedOptionalValue.Should().Be("foo");
  }

  private static ReflectionCustomAttributeData CreateCustomAttributeData<TAttributeType>(
    IEnumerable<object?>? constructorArguments = null,
    IEnumerable<CustomAttributeNamedArgument>? namedArguments = null
  ) where TAttributeType : Attribute {
    constructorArguments ??= Enumerable.Empty<object>();
    namedArguments ??= Enumerable.Empty<CustomAttributeNamedArgument>();

    var positionalArguments = constructorArguments.Select(x => new CustomAttributeTypedArgument(x));

    return CreateCustomAttributeData(
      typeof(TAttributeType),
      positionalArguments.ToList(),
      namedArguments.ToList()
    );
  }

  private static ReflectionCustomAttributeData CreateCustomAttributeData(
    Type attributeType,
    IList<CustomAttributeTypedArgument>? constructorArguments = null,
    IList<CustomAttributeNamedArgument>? namedArguments = null
  ) {
    constructorArguments ??= Array.Empty<CustomAttributeTypedArgument>();
    namedArguments ??= Array.Empty<CustomAttributeNamedArgument>();

    return new ReflectionCustomAttributeData(
      attributeType,
      constructorArguments,
      namedArguments
    );
  }

  private static CustomAttributeNamedArgument CreateNamedArgument<TAttribute>(
    Expression<MemberExpressionFunc<TAttribute, object>> nameSelector,
    object value
  ) {
    return CreateNamedArgument(nameSelector, new CustomAttributeTypedArgument(value));
  }

  private static CustomAttributeNamedArgument CreateNamedArgument<TAttribute>(
    Expression<MemberExpressionFunc<TAttribute, object>> nameSelector,
    CustomAttributeTypedArgument value
  ) {
    var memberName = nameSelector.GetMemberExpressionName();
    var attributeMember = typeof(TAttribute).GetMember(memberName).Single();
    return new CustomAttributeNamedArgument(
      attributeMember,
      value
    );
  }
}

internal sealed class NoCtorParametersAttribute : Attribute {
  public int FieldValue;
  public int? NamedValue { get; set; }

  public void MethodTarget() { }
}

internal sealed class WithSingleCtorParametersAttribute : Attribute {
  public WithSingleCtorParametersAttribute(int value) {
    Value = value;
  }

  public int Value { get; }
}

internal sealed class WithMultipleCtorParametersAttribute : Attribute {
  public WithMultipleCtorParametersAttribute(int value, string? optionalValue = null) {
    Value = value;
    OptionalValue = optionalValue;
  }

  public int Value { get; }

  public string? OptionalValue { get; }

  public int NamedValue { get; set; }

  public string? NamedOptionalValue { get; set; }
}
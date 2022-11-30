using System.Reflection;

namespace Mumei.Common.Reflection;

public sealed class ReflectionCustomAttributeData : CustomAttributeData {
  public ReflectionCustomAttributeData(
    Type attributeType,
    IList<CustomAttributeTypedArgument> constructorArguments,
    IList<CustomAttributeNamedArgument> namedArguments
  ) {
    Type = attributeType;
    ConstructorArguments = constructorArguments;
    NamedArguments = namedArguments;
    CanBeConstructed = attributeType.IsRuntimeType();
  }

  public Type Type { get; }
  public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }
  public override IList<CustomAttributeNamedArgument> NamedArguments { get; }

  public bool CanBeConstructed { get; }

  public object CreateInstance() {
    if (!CanBeConstructed) {
      throw new NotSupportedException("Cannot create instance of compile-time attribute");
    }

    var constructor = Type.GetConstructors().First();
    var constructorParameters = constructor.GetParameters();
    var constructorArguments = new object[constructorParameters.Length];
    for (var i = 0; i < constructorParameters.Length; i++) {
      var argument = ConstructorArguments[i];
      constructorArguments[i] = argument;
    }

    var instance = constructor.Invoke(constructorArguments);
    foreach (var namedArgument in NamedArguments) {
      SetMemberValue(instance, namedArgument.MemberInfo, namedArgument.TypedValue);
    }

    return instance;
  }

  private void SetMemberValue(object instance, MemberInfo member, object value) {
    switch (member) {
      case PropertyInfo property:
        property.SetValue(instance, value);
        break;
      case FieldInfo field:
        field.SetValue(instance, value);
        break;
      default:
        throw new NotSupportedException($"Cannot set value of member {member}");
    }
  }
}

public static class AttributeDataExtensions {
 
}
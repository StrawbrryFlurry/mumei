using System.Reflection;
using Mumei.Common.Utilities;

namespace Mumei.Common.Reflection;

internal static class ReflectionAttributeFactory {
  public static object CreateInstance(CustomAttributeData data) {
    if (!data.AttributeType.IsRuntimeType()) {
      throw new NotSupportedException("Cannot create instance of compile-time attribute");
    }

    return CreateAttributeInstanceCore(data);
  }

  private static object CreateAttributeInstanceCore(CustomAttributeData data) {
    var type = data.AttributeType;

    var instance = CreateAttributeObjectInstance(type, data.ConstructorArguments);

    foreach (var namedArgument in data.NamedArguments) {
      SetAttributeMemberValue(instance, namedArgument.MemberInfo, namedArgument.TypedValue);
    }

    return instance;
  }

  private static object CreateAttributeObjectInstance(Type type, IList<CustomAttributeTypedArgument> arguments) {
    var constructor = type.GetConstructor(arguments.Select(x => x.ArgumentType).ToArray());

    if (constructor is null) {
      var argumentNames = arguments.Select(x => x.ArgumentType.FullName).JoinBy(",");
      throw new NotSupportedException(
        $"Cannot find suitable constructor for attribute {type.FullName} " +
        $"with arguments {argumentNames}");
    }

    return constructor.Invoke(arguments.Select(x => x.Value).ToArray());
  }

  private static void SetAttributeMemberValue(object instance, MemberInfo member, object value) {
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
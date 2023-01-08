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
      SetAttributeMemberValue(instance, namedArgument.MemberInfo, namedArgument.TypedValue.Value);
    }

    return instance;
  }

  private static object CreateAttributeObjectInstance(Type type, IList<CustomAttributeTypedArgument> arguments) {
    var constructor = GetCtorOverloadForParameters(type, arguments, out var ctorArguments);

    if (constructor is not null) {
      return constructor.Invoke(ctorArguments);
    }

    var argumentNames = arguments.Select(x => x.ArgumentType.FullName).JoinBy(",");
    throw new MissingMethodException(
      $"Cannot find suitable constructor for attribute {type.FullName} " +
      $"with arguments {argumentNames}"
    );
  }

  private static ConstructorInfo? GetCtorOverloadForParameters(
    Type type,
    IList<CustomAttributeTypedArgument> arguments,
    out object?[] ctorArguments
  ) {
    ctorArguments = null!;
    var constructors = type.GetConstructors(
      BindingFlags.Public
      | BindingFlags.Instance
      | BindingFlags.DeclaredOnly
      | BindingFlags.NonPublic
    );

    foreach (var ctor in constructors) {
      var ctorParameters = ctor.GetParameters();

      if (!IsMatchingParameterOverload(ctorParameters, arguments)) {
        continue;
      }

      ctorArguments = GetCtorArguments(ctorParameters, arguments);
      return ctor;
    }

    return null;
  }

  private static bool IsMatchingParameterOverload(
    IReadOnlyList<ParameterInfo> ctorParameters,
    IList<CustomAttributeTypedArgument> arguments
  ) {
    if (!HasMatchingParameterCount(ctorParameters, arguments)) {
      return false;
    }

    for (var i = 0; i < ctorParameters.Count; i++) {
      var ctorParameter = ctorParameters[i];
      var isRequiredParameter = !ctorParameter.IsOptional;

      if (i >= arguments.Count && !isRequiredParameter) {
        continue;
      }

      if (i >= arguments.Count && isRequiredParameter) {
        return false;
      }

      var argument = arguments[i];

      if (!argument.ArgumentType.IsAssignableTo(ctorParameter.ParameterType)) {
        return false;
      }
    }

    return true;
  }

  private static bool HasMatchingParameterCount(
    IReadOnlyCollection<ParameterInfo> ctorParameters,
    ICollection<CustomAttributeTypedArgument> arguments
  ) {
    var optionalParameterCount = ctorParameters.Count(p => p.IsOptional);

    if (arguments.Count == ctorParameters.Count) {
      return true;
    }

    var requiredParametersCount = ctorParameters.Count - optionalParameterCount;
    return requiredParametersCount <= arguments.Count && arguments.Count <= ctorParameters.Count;
  }

  private static object?[] GetCtorArguments(
    IReadOnlyList<ParameterInfo> ctorParameters,
    IList<CustomAttributeTypedArgument> arguments
  ) {
    var ctorArguments = new object?[ctorParameters.Count];

    for (var i = 0; i < ctorParameters.Count; i++) {
      var isRequiredParameter = !ctorParameters[i].IsOptional;
      var hasArgument = i < arguments.Count;

      if (!isRequiredParameter && !hasArgument) {
        ctorArguments[i] = null;
        continue;
      }

      var argument = arguments[i];
      ctorArguments[i] = argument.Value;
    }

    return ctorArguments;
  }

  private static void SetAttributeMemberValue(object instance, MemberInfo member, object? value) {
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
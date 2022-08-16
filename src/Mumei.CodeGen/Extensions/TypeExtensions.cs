using System.Reflection;
using System.Runtime.Serialization;

namespace Mumei.CodeGen.Extensions;

public static class TypeExtensions {
  private const string AttributeSuffix = "Attribute";

  public static string GetAttributeName(this Type attributeType) {
    var name = attributeType.Name;

    if (name.EndsWith(AttributeSuffix)) {
      name = name.Substring(0, name.Length - AttributeSuffix.Length);
    }

    return name;
  }

  /// <summary>
  ///   Gets the default value of the type as defined in
  /// </summary>
  /// <param name="type">The type of which the default value should be created</param>
  /// <returns></returns>
  internal static object GetDefaultValue(this Type type) {
    if (!type.IsValueType) {
      return null!;
    }

    return GetDefaultValueForValueType(type);
  }

  private static object GetDefaultValueForValueType(Type type) {
    if (IsNullableType(type)) {
      return null!;
    }

    return FormatterServices.GetUninitializedObject(type);
  }

  internal static bool IsNullableType(this Type type) {
    return Nullable.GetUnderlyingType(type) is not null;
  }

  internal static MethodInfo SelectGenericMethodOverload(this Type type, string methodName) {
    try {
      return type.GetMethods().Single(m => m.Name == methodName && m.IsGenericMethod);
    }
    catch {
      throw new InvalidOperationException(
        $"Type {type.FullName} has more than one generic method with name {methodName}"
      );
    }
  }
}
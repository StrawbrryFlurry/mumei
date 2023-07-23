using System.Reflection;

namespace Mumei.Common.Utilities;

public static class ReflectionExtensions {
  public static bool IsAssignableTo(this Type t, Type type) {
    return type.IsAssignableFrom(t);
  }

  public static TAttribute? GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute {
    return type.GetCustomAttribute<TAttribute>();
  }

  public static TAttribute? GetAttribute<TAttribute>(this MemberInfo member) where TAttribute : Attribute {
    return member.GetCustomAttribute<TAttribute>();
  }

  public static FieldInfo GetBackingField(this PropertyInfo property) {
    var backingFieldName = $"<{property.Name}>k__BackingField";
    return property.DeclaringType!.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
  }
}
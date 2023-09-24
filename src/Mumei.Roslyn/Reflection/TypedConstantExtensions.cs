using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public static class TypedConstantExtensions {
  public static T? GetValue<T>(this TypedConstant constant) {
    if (constant.Value is T value) {
      return value;
    }

    return default;
  }
}
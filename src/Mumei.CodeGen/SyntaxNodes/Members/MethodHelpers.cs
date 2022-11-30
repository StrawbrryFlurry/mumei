namespace Mumei.CodeGen.SyntaxNodes;

public static class MethodHelpers {
  public static Type GetFunctionDefinition(Type functionType, out Type[] arguments) {
    if (functionType == typeof(Action)) {
      arguments = Type.EmptyTypes;
      return typeof(void);
    }

    if (!functionType.IsGenericType) {
      throw new ArgumentException($"Argument of type {functionType} is not a Action or Func");
    }

    var genericType = functionType.GetGenericTypeDefinition();
    var genericArguments = functionType.GetGenericArguments();

    if (IsAction(genericType)) {
      arguments = genericArguments;
      return typeof(void);
    }

    if (IsFunc(genericType)) {
      arguments = genericArguments.Take(genericArguments.Length - 1).ToArray();
      return genericArguments.Last();
    }

    throw new ArgumentException($"Argument of type {functionType} is not a Action or Func");
  }

  private static bool IsFunc(Type type) {
    return type switch {
      _ when type == typeof(Func<>) => true,
      _ when type == typeof(Func<,>) => true,
      _ when type == typeof(Func<,,>) => true,
      _ when type == typeof(Func<,,,>) => true,
      _ when type == typeof(Func<,,,,>) => true,
      _ when type == typeof(Func<,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,,,,,,,,>) => true,
      _ when type == typeof(Func<,,,,,,,,,,,,,,,,>) => true,
      _ => false
    };
  }

  private static bool IsAction(Type type) {
    return type switch {
      _ when type == typeof(Action<>) => true,
      _ when type == typeof(Action<,>) => true,
      _ when type == typeof(Action<,,>) => true,
      _ when type == typeof(Action<,,,>) => true,
      _ when type == typeof(Action<,,,,>) => true,
      _ when type == typeof(Action<,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,,,,,,,,>) => true,
      _ when type == typeof(Action<,,,,,,,,,,,,,,,>) => true,
      _ => false
    };
  }
}
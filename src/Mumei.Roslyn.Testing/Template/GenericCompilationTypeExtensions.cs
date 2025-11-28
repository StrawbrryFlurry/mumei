using System.Collections.Immutable;

namespace Mumei.Roslyn.Testing.Template;

public static class GenericCompilationTypeExtensions {
  public static ConstructedCompilationGenericType Args(
    this Type openGenericType,
    params CompilationType[] typeArgument
  ) {
    return new ConstructedCompilationGenericType {
      OpenGenericType = openGenericType,
      Arguments = ImmutableArray.Create(typeArgument)
    };
  }

  public static CompilationCallExpression Call(
    this Type type,
    params object?[] arguments
  ) {
    return new CompilationCallExpression {
      Arguments = arguments,
      Target = new CompilationTypeFormattable(type)
    };
  }

  public static CompilationCallExpression Call(
    this ConstructedCompilationGenericType type,
    params object?[] arguments
  ) {
    return new CompilationCallExpression {
      Arguments = arguments,
      Target = type
    };
  }

  public static CompilationCallExpression Call(
    this CompilationType type,
    params object?[] arguments
  ) {
    return new CompilationCallExpression {
      Arguments = arguments,
      Target = type
    };
  }
}
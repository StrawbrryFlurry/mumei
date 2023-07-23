namespace Mumei.Common;

public static class TypeExtensions {
  private const string RuntimeTypeFqn = "System.RuntimeType";
  private static readonly Type RuntimeType = Type.GetType(RuntimeTypeFqn)!;

  public static bool IsRuntimeType(this Type type) {
    // ReSharper disable once PossibleMistakenCallToGetType.2
    return type.GetType() == RuntimeType;
  }
}
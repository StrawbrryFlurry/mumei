namespace Mumei.CodeGen.Extensions;

public static class EnumerableExtensions {
  public static string JoinBy<T>(this IEnumerable<T> enumerable, string separator) {
    return string.Join(separator, enumerable);
  }
}
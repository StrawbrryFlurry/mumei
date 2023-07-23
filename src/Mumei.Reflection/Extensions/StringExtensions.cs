namespace Mumei.Common.Extensions; 

public static class StringExtensions {
  public static string JoinBy<T>(this IEnumerable<T> source, string separator) {
    return string.Join(separator, source);
  }
}
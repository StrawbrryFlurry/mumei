namespace Mumei.Common.Utilities; 

public static class StringExtensions {
  public static string JoinBy<T>(this IEnumerable<T> source, string separator) {
    return string.Join(separator, source);
  }
}
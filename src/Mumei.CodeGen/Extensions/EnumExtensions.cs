namespace Mumei.CodeGen.Extensions;

public static class EnumExtensions {
  public static IEnumerable<T> GetFlags<T>(this T @enum) where T : Enum {
    var flags = new List<T>();
    var values = Enum.GetValues(@enum.GetType()) as T[];

    // Enum is empty
    if (values is null) {
      return flags;
    }

    foreach (var value in values) {
      if (@enum.HasFlag(value)) {
        flags.Add(value);
      }
    }

    return flags;
  }
}
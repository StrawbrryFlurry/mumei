using System.Runtime.CompilerServices;

namespace Mumei.Roslyn.Testing.Template;

internal static class CompilationTypeExtensions {
  public static string GetNameWithoutGenericArity(this Type type) {
    var name = type.FullName ?? type.Name;
    if (!type.IsGenericType) {
      return name;
    }

    var actualName = name.AsSpan()[..name.IndexOf('`')];
    return actualName.ToString();
  }

  public static string GetDisplayName(this Type type) {
    var name = type.FullName ?? type.Name;
    if (!type.IsGenericType) {
      return name;
    }

    if (!type.IsConstructedGenericType) {
      return type.GetNameWithoutGenericArity();
    }

    var nameBuilder = new DefaultInterpolatedStringHandler(name.Length, 0);
    var nameWithoutArity = type.GetNameWithoutGenericArity();
    nameBuilder.AppendLiteral(nameWithoutArity);
    nameBuilder.AppendLiteral("<");
    var genericArguments = type.GetGenericArguments();
    for (var i = 0; i < genericArguments.Length; i++) {
      var genericArgument = genericArguments[i];
      nameBuilder.AppendLiteral(genericArgument.GetDisplayName());

      if (i < genericArguments.Length - 1) {
        nameBuilder.AppendLiteral(", ");
      }
    }

    nameBuilder.AppendLiteral(">");
    return nameBuilder.ToStringAndClear();
  }
}
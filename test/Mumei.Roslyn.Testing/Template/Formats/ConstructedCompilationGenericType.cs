using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mumei.Roslyn.Testing.Template;

public readonly struct ConstructedCompilationGenericType : ITemplateFormattable {
  public required Type OpenGenericType { get; init; }
  public required ImmutableArray<CompilationType> Arguments { get; init; }

  public IEnumerable<CompilationType> ReferencedSources => Arguments;
  public IEnumerable<Type> ReferencedTypes => ImmutableArray.Create(OpenGenericType);

  public string ToString(string? format, IFormatProvider? formatProvider) {
    var actualName = OpenGenericType.GetNameWithoutGenericArity();
    var s = new DefaultInterpolatedStringHandler(actualName.Length, 0);
    s.AppendLiteral(actualName);
    s.AppendLiteral("<");
    foreach (var argument in Arguments) {
      s.AppendFormatted(argument.ToString(CompilationTemplateFormat.Display, null));
    }

    s.AppendLiteral(">");
    var display = s.ToStringAndClear();

    if (OpenGenericType.BaseType == typeof(Attribute)) {
      format ??= CompilationTemplateFormat.Attribute;
    }

    return format switch {
      CompilationTemplateFormat.Display => display,
      CompilationTemplateFormat.Attribute => $"[{display}]",
      _ => display
    };
  }
}
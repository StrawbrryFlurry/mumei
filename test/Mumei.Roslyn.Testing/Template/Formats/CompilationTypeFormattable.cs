using System.Collections.Immutable;

namespace Mumei.Roslyn.Testing.Template;

public readonly struct CompilationTypeFormattable : ITemplateFormattable {
  private readonly Type _type;

  public IEnumerable<CompilationType> ReferencedSources => ImmutableArray<CompilationType>.Empty;
  public IEnumerable<Type> ReferencedTypes => ImmutableArray.Create(_type);

  public CompilationTypeFormattable(Type type) {
    _type = type;
  }

  public string ToString(string? format, IFormatProvider? formatProvider) {
    if (_type.BaseType == typeof(Attribute)) {
      format ??= CompilationTemplateFormat.Attribute;
    }

    return format switch {
      CompilationTemplateFormat.Display => _type.GetDisplayName(),
      CompilationTemplateFormat.Attribute => $"[{_type.GetDisplayName()}]",
      _ => _type.GetDisplayName()
    };
  }
}
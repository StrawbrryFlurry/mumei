using System.Collections.Immutable;

namespace Mumei.Roslyn.Testing.Template;

public readonly struct CompilationCallExpression : ITemplateFormattable {
  public required object?[] Arguments { get; init; }

  public required ITemplateFormattable Target { get; init; }

  private readonly List<Type> _referencedTypes;
  private readonly List<CompilationType> _referencedSources;

  public CompilationCallExpression() {
    _referencedTypes = new List<Type>();
    _referencedSources = new List<CompilationType>();
  }

  public IEnumerable<CompilationType> ReferencedSources => Target.ReferencedSources.Concat(_referencedSources);
  public IEnumerable<Type> ReferencedTypes => Target.ReferencedTypes.Concat(_referencedTypes);

  public string ToString(string? format, IFormatProvider? formatProvider) {
    var target = Target.ToString(CompilationTemplateFormat.Display, null);
    var arguments = FormatArguments();
    var expr = $"{target}({arguments})";

    return format switch {
      CompilationTemplateFormat.Display => expr,
      CompilationTemplateFormat.Attribute => $"[{expr}]",
      _ => expr
    };
  }

  private string FormatArguments() {
    var formattedArguments = new string[Arguments.Length];
    for (var i = 0; i < Arguments.Length; i++) {
      var argument = Arguments[i];
      formattedArguments[i] = FormatArgument(argument);
    }

    return string.Join(", ", formattedArguments);
  }

  private string FormatArgument(object? o) {
    return o switch {
      null => "null",
      string s => $"\"{s}\"",
      Type t => FormatTypeArgument(t),
      ITemplateFormattable formattable => FormatTemplateArgument(formattable),
      _ => o.ToString()!
    };
  }

  private string FormatTypeArgument(Type t) {
    _referencedTypes.Add(t);
    return new CompilationTypeFormattable(t).ToString(CompilationTemplateFormat.Display, null);
  }

  private string FormatTemplateArgument(ITemplateFormattable formattable) {
    _referencedSources.AddRange(formattable.ReferencedSources);
    _referencedTypes.AddRange(formattable.ReferencedTypes);
    return formattable.ToString(CompilationTemplateFormat.Display, null);
  }
}
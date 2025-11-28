namespace Mumei.Roslyn.Testing.Template;

public interface ITemplateFormattable : IFormattable {
  /// <summary>
  /// All sources that this template references (including itself if it is a source)
  /// </summary>
  public IEnumerable<CompilationType> ReferencedSources { get; }

  /// <summary>
  /// All types that this template references (including itself if it is a type)
  /// </summary>
  public IEnumerable<Type> ReferencedTypes { get; }
}
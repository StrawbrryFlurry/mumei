using System.Collections.Immutable;
using Mumei.Roslyn.Testing.Template;

namespace Mumei.Roslyn.Testing;

public readonly struct TypeSource {
  public required string Name { get; init; }
  public required string Source { get; init; }
  public required ImmutableArray<Type> TypeReferences { get; init; }
  public required ImmutableArray<CompilationType> SourceReferences { get; init; }
}
using System.Collections.Immutable;

namespace Mumei.Roslyn.Testing.Template;

public readonly struct TypeSource {
    public required string Name { get; init; }
    public required string FullName { get; init; }
    public required string MetadataName { get; init; }
    public required string Text { get; init; }
    public required ImmutableArray<Type> TypeReferences { get; init; }
    public required ImmutableArray<CompilationType> SourceReferences { get; init; }
}
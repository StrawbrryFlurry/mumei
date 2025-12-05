using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public readonly struct CompilationFromSyntaxTree(Compilation compilation, string sourceNamespace) {
    public Compilation Compilation { get; } = compilation;
    public string? SourceNamespace { get; } = sourceNamespace;

    public INamedTypeSymbol FindType<T>() {
        var typeFullName = $"{SourceNamespace}.{typeof(T).Name}";
        var typeSymbol = Compilation.GetTypeByMetadataName(typeFullName);
        if (typeSymbol is null) {
            throw new InvalidOperationException($"Type '{typeFullName}' not found in compilation.");
        }

        return typeSymbol;
    }
}
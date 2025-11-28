using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public sealed class AssemblyCompilationReference : ICompilationReference {
    public required string AssemblyName { get; init; }
    public required string TypeName { get; init; }

    public void AddToCompilation(List<SyntaxTree> syntaxTreesRef, MetadataReferenceCollection metadataRef) {
        metadataRef.AddReference(AssemblyName);
    }
}
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public interface ICompilationReference {
    public void AddToCompilation(List<SyntaxTree> syntaxTreesRef, MetadataReferenceCollection metadataRef);
}

public interface IRootCompilationReference : ICompilationReference {
    public string SourceNamespace { get; }
}
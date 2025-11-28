using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public interface ICompilationReference {
    public void AddToCompilation(List<SyntaxTree> syntaxTreesRef, MetadataReferenceCollection metadataRef);
}
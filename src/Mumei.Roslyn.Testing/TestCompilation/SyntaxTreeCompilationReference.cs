using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mumei.Roslyn.Testing;

public sealed class SyntaxTreeCompilationReference : ICompilationReference {
    public required string TypeName { get; init; }
    public required string SourceCode { get; init; }
    public required ImmutableArray<ICompilationReference> References { get; init; }

    public void AddToCompilation(List<SyntaxTree> syntaxTreesRef, MetadataReferenceCollection metadataRef) {
        AddToCompilationCore(syntaxTreesRef, metadataRef, new HashSet<string>());
    }

    private void AddToCompilationCore(List<SyntaxTree> syntaxTreesRef, MetadataReferenceCollection metadataRef, HashSet<string> seenTexts) {
        if (seenTexts.Add(TypeName)) {
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceCode, path: TypeName);
            syntaxTreesRef.Add(syntaxTree);
        }

        foreach (var reference in References) {
            if (reference is AssemblyCompilationReference assemblyRef) {
                assemblyRef.AddToCompilation(syntaxTreesRef, metadataRef);
                continue;
            }

            if (reference is SyntaxTreeCompilationReference sourceRef) {
                sourceRef.AddToCompilationCore(syntaxTreesRef, metadataRef, seenTexts);
            }
        }
    }
}
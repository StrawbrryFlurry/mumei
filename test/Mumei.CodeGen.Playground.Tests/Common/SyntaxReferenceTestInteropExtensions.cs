using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Roslyn.Testing;
using Mumei.Roslyn.Testing;

namespace Mumei.CodeGen.Playground.Tests.Common {
    public static class SyntaxReferenceTestInteropExtensions {
        extension(GeneratorTest) {
            // public static GeneratorTest For<TGenerator>(ICompilationReference)
        }
    }
}

namespace Mumei.Roslyn.Testing {
    public interface ICompilationReference {
        // public void AddToCompilation(List<SyntaxTree> syntaxTreesRef, MetadataReferenceCollection metadataRef);
    }
}
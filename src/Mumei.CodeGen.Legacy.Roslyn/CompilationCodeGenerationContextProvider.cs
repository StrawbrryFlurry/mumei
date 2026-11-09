using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Legacy.Components;

namespace Mumei.CodeGen.Legacy.Roslyn;

internal sealed class CompilationCodeGenerationContextProvider(Compilation compilation)
    : ICodeGenerationContextProvider {
    public Compilation Compilation => compilation;
}
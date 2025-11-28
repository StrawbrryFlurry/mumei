using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn;

internal sealed class CompilationCodeGenerationContextProvider(Compilation compilation) : ICodeGenerationContextProvider {
    public Compilation Compilation => compilation;
}
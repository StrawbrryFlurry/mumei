using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;

namespace Mumei.CodeGen.Roslyn;

internal sealed class CompilationSynthesisProvider(Compilation compilation) : ISynthesisProvider {
    public Compilation Compilation => compilation;
}
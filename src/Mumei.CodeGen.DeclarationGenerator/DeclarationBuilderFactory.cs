using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.DeclarationGenerator;

internal sealed class DeclarationBuilderFactory {
    public BlockBuilder CreateFieldFromDefinition(
        IFieldSymbol field
    ) { }

    public BlockBuilder CreateMethodFromDefinition(
        IMethodSymbol method
    ) { }
}
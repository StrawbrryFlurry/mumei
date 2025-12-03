using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Roslyn.Components;

internal sealed class RoslynSyntheticType(ITypeSymbol t) : ISyntheticType, ISyntheticConstructable<TypeInfoFragment> {
    public SyntheticIdentifier Name { get; } = t.Name;

    public TypeInfoFragment Construct(ICompilationUnitContext compilationUnit) {
        return t.ToRenderFragment();
    }
}
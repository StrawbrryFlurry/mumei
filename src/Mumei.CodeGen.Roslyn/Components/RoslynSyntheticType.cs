using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.CodeGen.Roslyn.RendererExtensions;

namespace Mumei.CodeGen.Roslyn.Components;

internal sealed class RoslynSyntheticType(ITypeSymbol t) : ISyntheticType, ISyntheticConstructable<TypeInfoFragment> {
    public string Name => t.Name;

    public TypeInfoFragment Construct(ICompilationUnitContext compilationUnit) {
        return t.ToRenderFragment();
    }
}
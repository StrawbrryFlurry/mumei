using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Roslyn.Components;

internal sealed class RoslynSyntheticType(ITypeSymbol t) : ISyntheticType, ISyntheticConstructable<TypeInfoFragment> {
    public SyntheticIdentifier Name { get; } = t.Name;

    public ITypeSymbol UnderlyingType => t;

    public TypeInfoFragment Construct(ICompilationUnitContext compilationUnit) {
        return t.ToRenderFragment();
    }

    public bool Equals(ISyntheticType other) {
        if (other is RoslynSyntheticType roslynType) {
            return SymbolEqualityComparer.Default.Equals(UnderlyingType, roslynType.UnderlyingType);
        }

        if (other is RuntimeSyntheticType runtimeType) {
            return runtimeType.UnderlyingType.FullName == UnderlyingType.ToFullNameFast();
        }

        return false;
    }
}
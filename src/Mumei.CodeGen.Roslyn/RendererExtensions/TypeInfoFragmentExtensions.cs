using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Roslyn;

internal static class TypeInfoFragmentExtensions {
    extension(ITypeSymbol type) {
        public TypeInfoFragment ToRenderFragment() {
            return new TypeInfoFragment(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), type.NullableAnnotation == NullableAnnotation.Annotated, false);
        }
    }
}
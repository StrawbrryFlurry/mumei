using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Roslyn;

public static class SymbolAttributeExtensions {
    extension(ISymbol symbol) {
        public bool HasAttribute(INamedTypeSymbol attributeSymbol) {
            foreach (var attributeData in symbol.GetAttributes()) {
                if (SymbolEqualityComparer.Default.Equals(attributeData.AttributeClass, attributeSymbol)) {
                    return true;
                }
            }

            return false;
        }
    }
}
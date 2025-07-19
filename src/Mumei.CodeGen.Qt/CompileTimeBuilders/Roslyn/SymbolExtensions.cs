using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt.Roslyn;

internal static class SymbolExtensions {
    public static ITypeSymbol DeclaringType(this IMethodSymbol method) {
        return method.ContainingType;
    }
}
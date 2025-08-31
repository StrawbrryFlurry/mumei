using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Qt.Roslyn;

internal static class SymbolExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ITypeSymbol DeclaringType(this IMethodSymbol method) {
        return method.ContainingType;
    }
}
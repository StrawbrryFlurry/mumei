using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Roslyn.Components;

internal static class MethodSignatureFactory {
    private const int MaxDelegateArity = 16;

    public static ITypeSymbol MakeMethodSignatureDelegateType(
        IMethodSymbol methodSymbol,
        Compilation compilation
    ) {
        var arity = methodSymbol.ReturnsVoid
            ? methodSymbol.Parameters.Length
            : methodSymbol.Parameters.Length + 1;

        if (arity > MaxDelegateArity) {
            return compilation.GetSpecialType(SpecialType.System_Delegate);
        }

        INamedTypeSymbol symbolToConstruct;
        if (methodSymbol.ReturnsVoid) {
            symbolToConstruct = compilation.GetTypeByMetadataName($"System.Action`{arity}")!;
        } else {
            symbolToConstruct = compilation.GetTypeByMetadataName($"System.Func`{arity}")!;
        }

        var typeArguments = new ITypeSymbol[arity];
        for (var i = 0; i < methodSymbol.Parameters.Length; i++) {
            var parameter = methodSymbol.Parameters[i];
            typeArguments[i] = parameter.Type;
        }

        if (!methodSymbol.ReturnsVoid) {
            typeArguments[arity - 1] = methodSymbol.ReturnType;
        }

        return symbolToConstruct!.Construct(typeArguments);
    }
}
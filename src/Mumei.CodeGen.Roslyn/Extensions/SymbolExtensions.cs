using Microsoft.CodeAnalysis;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.Roslyn;

internal static class SymbolExtensions {
    extension(INamespaceOrTypeSymbol symbol) {
        public string ToFullNameFast() {
            return FullyQualifyNameFast(symbol);
        }
    }

    private static string FullyQualifyNameFast(INamespaceOrTypeSymbol symbol) {
        const int averageQualifierParts = 4;

        if (symbol is INamespaceSymbol { IsGlobalNamespace: true }) {
            return "";
        }

        if (symbol.ContainingType is null && symbol.ContainingNamespace is { IsGlobalNamespace: true }) {
            return symbol.Name;
        }

        if (symbol.ContainingType is not null) {
            var containingTypeFullName = FullyQualifyNameFast(symbol.ContainingType);
            return $"{containingTypeFullName}.{symbol.Name}";
        }

        if (symbol.ContainingNamespace is null) {
            return symbol.Name;
        }

        var qualifiedNamePartsBuilder = new ArrayBuilder<string>(averageQualifierParts);
        qualifiedNamePartsBuilder.Add(symbol.Name);
        var currentNamespace = symbol.ContainingNamespace;
        while (currentNamespace is not null && !currentNamespace.IsGlobalNamespace) {
            qualifiedNamePartsBuilder.Add(currentNamespace.Name);
            currentNamespace = currentNamespace.ContainingNamespace;
        }

        var namespaceBuilder = new ArrayBuilder<char>(stackalloc char[ArrayBuilder.InitSize]);
        for (var i = qualifiedNamePartsBuilder.Count - 1; i >= 0; i--) {
            if (i < qualifiedNamePartsBuilder.Count - 1) {
                namespaceBuilder.Add('.');
            }

            namespaceBuilder.AddRange(qualifiedNamePartsBuilder.Elements[i]);
        }

        qualifiedNamePartsBuilder.Dispose();
        return namespaceBuilder.ToStringAndFree();
    }
}
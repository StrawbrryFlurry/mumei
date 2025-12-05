using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.Roslyn.Testing.Extensions;

public static class SyntaxNodeExtensions {
    extension(ISymbol symbol) {
        public MethodDeclarationSyntax GetDeclaration() {
            return symbol.GetDeclaration<MethodDeclarationSyntax>();
        }

        public TDeclaration GetDeclaration<TDeclaration>()
            where TDeclaration : SyntaxNode {
            var declarationSyntax = symbol.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax())
                .OfType<TDeclaration>()
                .FirstOrDefault();

            if (declarationSyntax is null) {
                throw new InvalidOperationException(
                    $"Declaration of type '{typeof(TDeclaration).Name}' not found for symbol '{symbol.Name}'.");
            }

            return declarationSyntax;
        }
    }

    extension(INamedTypeSymbol type) {
        public IMethodSymbol GetMethod(string methodName) {
            var methodSymbol = type.GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(method => method.Name == methodName);

            if (methodSymbol is null) {
                throw new InvalidOperationException(
                    $"Method '{methodName}' not found in type '{type.Name}'.");
            }

            return methodSymbol;
        }

        public IPropertySymbol GetProperty(string propertyName) {
            var propertySymbol = type.GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(prop => prop.Name == propertyName);

            if (propertySymbol is null) {
                throw new InvalidOperationException(
                    $"Property '{propertyName}' not found in type '{type.Name}'.");
            }

            return propertySymbol;
        }

        public IFieldSymbol GetField(string fieldName) {
            var fieldSymbol = type.GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(field => field.Name == fieldName);

            if (fieldSymbol is null) {
                throw new InvalidOperationException(
                    $"Field '{fieldName}' not found in type '{type.Name}'.");
            }

            return fieldSymbol;
        }
    }

    extension(MethodDeclarationSyntax method) {
        public InvocationExpressionSyntax FindInvocationOf(string methodName) {
            var invocation = method.Body!.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(inv => {
                    if (inv.Expression is IdentifierNameSyntax identifierName) {
                        return identifierName.Identifier.Text == methodName;
                    } else if (inv.Expression is MemberAccessExpressionSyntax memberAccess) {
                        return memberAccess.Name.Identifier.Text == methodName;
                    }

                    return false;
                });

            if (invocation is null) {
                throw new InvalidOperationException(
                    $"Invocation of method '{methodName}' not found in the provided block.");
            }

            return invocation;
        }
    }
}
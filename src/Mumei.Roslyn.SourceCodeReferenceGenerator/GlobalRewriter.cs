using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.Roslyn.SourceCodeReferenceGenerator;

internal sealed class TypeToGloballyQualifiedIdentifierRewriter : CSharpSyntaxRewriter {
    private readonly SemanticModel _sm;
    private readonly ImmutableArray<ITypeSymbol>.Builder _typeReferences = ImmutableArray.CreateBuilder<ITypeSymbol>();

    private TypeToGloballyQualifiedIdentifierRewriter(SemanticModel sm) {
        _sm = sm;
    }

    public static T GlobalizeIdentifiers<T>(
        SemanticModel sm,
        T syntaxNode,
        out ImmutableArray<ITypeSymbol> typeReferences
    ) where T : SyntaxNode {
        var rewriter = new TypeToGloballyQualifiedIdentifierRewriter(sm);
        var resultNode = (T)rewriter.Visit(syntaxNode);
        typeReferences = rewriter._typeReferences.ToImmutable();
        return resultNode;
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
        if (node.Parent is QualifiedNameSyntax qualifiedNameSyntax) {
            // Ignore the right side of a qualified name e.g. `Class.SomeMember` since
            // fully expanding `Class` will already result in the fully qualified name
            if (qualifiedNameSyntax.Right == node) {
                return base.VisitIdentifierName(node)!;
            }

            // We're part of a nested qualified name, e.g. `Namespace.Class.Member`
            // only fully qualify the leftmost part of the qualified name
            if (qualifiedNameSyntax.Parent is QualifiedNameSyntax) {
                return base.VisitIdentifierName(node)!;
            }
        }

        if (!TryGetIdentifierType(node, out var identifierType)) {
            return base.VisitIdentifierName(node)!;
        }

        if (identifierType.ContainingNamespace.IsGlobalNamespace) {
            // We don't need to fully qualify types in the global namespace
            return base.VisitIdentifierName(node)!;
        }

        _typeReferences.Add(identifierType);
        return SyntaxFactory.IdentifierName(
            identifierType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        );
    }

    private bool TryGetIdentifierType(IdentifierNameSyntax identifierNode, out ITypeSymbol identifierType) {
        var identifier = _sm.GetSymbolInfo(identifierNode).Symbol;
        if (identifier is ITypeSymbol typeIdentifier) {
            identifierType = typeIdentifier;
            return true;
        }

        // Identifier is the Attribute type in an AttributeSyntax
        // e.g. `[Attribute]` or `[Attribute()]` where the
        // MethodSymbol is the Attribute's constructor
        if (identifier is IMethodSymbol attributeCtor) {
            identifierType = attributeCtor.ContainingType;
            return true;
        }

        identifierType = null!;
        return false;
    }
}
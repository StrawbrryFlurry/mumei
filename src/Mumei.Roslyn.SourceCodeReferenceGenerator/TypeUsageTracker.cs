using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.Roslyn.SourceCodeReferenceGenerator;

internal sealed class TypeUsageTracker : CSharpSyntaxWalker {
    private readonly SemanticModel _sm;
    private readonly ImmutableArray<ITypeSymbol>.Builder _typeReferences = ImmutableArray.CreateBuilder<ITypeSymbol>();
    private readonly ImmutableArray<IAliasSymbol>.Builder _aliases = ImmutableArray.CreateBuilder<IAliasSymbol>();

    private TypeUsageTracker(SemanticModel sm) {
        _sm = sm;
    }

    public static (ImmutableArray<ITypeSymbol>, ImmutableArray<IAliasSymbol>) FindUsedTypes<T>(
        SemanticModel sm,
        T syntaxNode
    ) where T : SyntaxNode {
        var rewriter = new TypeUsageTracker(sm);
        rewriter.Visit(syntaxNode);
        return (
            rewriter._typeReferences.ToImmutable(),
            rewriter._aliases.ToImmutable()
        );
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node) {
        if (node.Identifier.Text == "CustomAwaitable") {
            Debug.Indent();
        }

        TryTrackIdentifierType(_sm, node);
        base.VisitIdentifierName(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
        var targetMethod = _sm.GetSymbolInfo(node).Symbol as IMethodSymbol;
        if (targetMethod?.IsExtensionMethod ?? false) {
            _typeReferences.Add(targetMethod.ContainingType);
        }

        base.VisitInvocationExpression(node);
    }

    private void TryTrackIdentifierType(
        SemanticModel sm,
        IdentifierNameSyntax node
    ) {
        if (node.Parent is QualifiedNameSyntax qualifiedNameSyntax) {
            // Ignore the right side of a qualified name e.g. `Class.SomeMember` since
            // fully expanding `Class` will already result in the fully qualified name
            if (qualifiedNameSyntax.Right == node) {
                return;
            }

            // We're part of a nested qualified name, e.g. `Namespace.Class.Member`
            // only track the leftmost part of the qualified name
            if (qualifiedNameSyntax.Parent is QualifiedNameSyntax) {
                return;
            }
        }

        if (!TryGetIdentifierType(sm, node, out var identifierType)) {
            return;
        }

        _typeReferences.Add(identifierType);

        if (identifierType.ContainingNamespace.IsGlobalNamespace) {
            return;
        }
    }


    private bool TryGetIdentifierType(SemanticModel sm, IdentifierNameSyntax identifierNode, out ITypeSymbol identifierType) {
        var identifier = sm.GetSymbolInfo(identifierNode).Symbol;
        if (identifier is ITypeSymbol typeIdentifier) {
            if (sm.GetAliasInfo(identifierNode) is { } alias) {
                _aliases.Add(alias);
            }

            identifierType = typeIdentifier;
            return true;
        }

        // Identifier is the Attribute type in an AttributeSyntax
        // e.g. `[Attribute]` or `[Attribute()]` where the
        // MethodSymbol is the Attribute's constructor
        if (identifier is IMethodSymbol possibleAttributeCtor) {
            var attributeType = sm.Compilation.GetTypeByMetadataName("System.Attribute");
            var isAttribute = sm.Compilation.HasImplicitConversion(possibleAttributeCtor.ContainingType, attributeType);
            if (isAttribute) {
                identifierType = possibleAttributeCtor.ContainingType;
                return true;
            }
        }

        identifierType = null!;
        return false;
    }
}
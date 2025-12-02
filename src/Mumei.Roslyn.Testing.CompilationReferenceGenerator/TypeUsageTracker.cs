using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.Roslyn.Testing.CompilationReferenceGenerator;

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
        var visitor = new TypeUsageTracker(sm);
        visitor.Visit(syntaxNode);
        return (
            visitor._typeReferences.ToImmutable(),
            visitor._aliases.ToImmutable()
        );
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node) {
        TryTrackIdentifierType(node);
        base.VisitIdentifierName(node);
    }

    public override void VisitParameter(ParameterSyntax node) {
        if (node.Type is null) {
            return;
        }

        TryTrackType(node.Type);
        base.VisitParameter(node);
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node) {
        TryTrackType(node.Declaration.Type);
        base.VisitFieldDeclaration(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node) {
        TryTrackType(node.Type);
        base.VisitPropertyDeclaration(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
        var targetMethod = _sm.GetSymbolInfo(node).Symbol as IMethodSymbol;
        if (targetMethod?.IsExtensionMethod ?? false) {
            _typeReferences.Add(targetMethod.ContainingType);
        }

        base.VisitInvocationExpression(node);
    }

    public override void VisitVariableDeclaration(VariableDeclarationSyntax node) {
        if (node.Type.IsVar) {
            base.VisitVariableDeclaration(node);
            return;
        }

        var type = _sm.GetSymbolInfo(node.Type).Symbol as ITypeSymbol;
        if (type is null) {
            return;
        }

        _typeReferences.Add(type);
        base.VisitVariableDeclaration(node);
    }

    public override void VisitBaseList(BaseListSyntax node) {
        foreach (var baseType in node.Types) {
            TryTrackType(baseType.Type);
        }

        base.VisitBaseList(node);
    }

    private void TryTrackType(TypeSyntax typeSyntax) {
        var type = _sm.GetSymbolInfo(typeSyntax).Symbol as ITypeSymbol;
        if (type is null) {
            return;
        }

        TryTrackType(type);
    }

    private void TryTrackType(ITypeSymbol typeSymbol) {
        _typeReferences.Add(typeSymbol);
        if (typeSymbol is not INamedTypeSymbol namedType) {
            return;
        }

        if (!namedType.IsGenericType || namedType.IsUnboundGenericType) {
            return;
        }

        foreach (var typeArguments in namedType.TypeArguments) {
            TryTrackType(typeArguments);
        }
    }

    private void TryTrackIdentifierType(
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

        if (!TryGetIdentifierType(node, out var identifierType)) {
            return;
        }

        TryTrackType(identifierType);

        if (identifierType.ContainingNamespace?.IsGlobalNamespace ?? false) {
            return;
        }
    }


    private bool TryGetIdentifierType(IdentifierNameSyntax identifierNode, out ITypeSymbol identifierType) {
        var identifier = _sm.GetSymbolInfo(identifierNode).Symbol;
        if (identifier is ITypeSymbol typeIdentifier) {
            if (_sm.GetAliasInfo(identifierNode) is { } alias) {
                _aliases.Add(alias);
            }

            identifierType = typeIdentifier;
            return true;
        }

        // Identifier is the Attribute type in an AttributeSyntax
        // e.g. `[Attribute]` or `[Attribute()]` where the
        // MethodSymbol is the Attribute's constructor
        if (identifier is IMethodSymbol possibleAttributeCtor) {
            var attributeType = _sm.Compilation.GetTypeByMetadataName("System.Attribute");
            var isAttribute = _sm.Compilation.HasImplicitConversion(possibleAttributeCtor.ContainingType, attributeType);
            if (isAttribute) {
                identifierType = possibleAttributeCtor.ContainingType;
                return true;
            }
        }

        identifierType = null!;
        return false;
    }
}
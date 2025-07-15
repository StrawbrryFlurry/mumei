using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Mumei.CodeGen.Qt;

public class TypeToGloballyQualifiedIdentifierRewriter(
    SemanticModel sm
) : CSharpSyntaxRewriter {
    public static T GlobalizeIdentifiers<T>(
        SemanticModel sm,
        T syntaxNode
    ) where T : SyntaxNode {
        var rewriter = new TypeToGloballyQualifiedIdentifierRewriter(sm);
        var resultNode = (T)rewriter.Visit(syntaxNode);
        return resultNode;
    }

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node) {
        if (!TryGlobalizeIdentifier(sm, node, out var globalizedIdentifier)) {
            return base.VisitGenericName(node);
        }

        return globalizedIdentifier!;
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
        if (!TryGlobalizeIdentifier(sm, node, out var globalizedIdentifier)) {
            return base.VisitIdentifierName(node)!;
        }

        return globalizedIdentifier!;
    }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node) {
        if (node.Expression is not MemberAccessExpressionSyntax memberAccess) {
            return base.VisitInvocationExpression(node);
        }

        var targetMethod = ModelExtensions.GetSymbolInfo(sm, memberAccess.Name).Symbol as IMethodSymbol;
        if (!targetMethod?.IsExtensionMethod ?? false) {
            return base.VisitInvocationExpression(node);
        }

        var visited = base.VisitInvocationExpression(node);
        if (visited is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax targetMemberAccess } invocation) {
            return visited;
        }

        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(targetMethod.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                IdentifierName(targetMethod.Name)
            ),
            ArgumentList(SeparatedList([
                Argument(targetMemberAccess.Expression),
                ..invocation.ArgumentList.Arguments
            ]))
        );
    }

    public static bool TryGlobalizeIdentifier(
        SemanticModel sm,
        SimpleNameSyntax node,
        out IdentifierNameSyntax? globalizedIdentifier
    ) {
        globalizedIdentifier = null!;
        if (node.Parent is QualifiedNameSyntax qualifiedNameSyntax) {
            // Ignore the right side of a qualified name e.g. `Class.SomeMember` since
            // fully expanding `Class` will already result in the fully qualified name
            if (qualifiedNameSyntax.Right == node) {
                return false!;
            }

            // We're part of a nested qualified name, e.g. `Namespace.Class.Member`
            // only fully qualify the leftmost part of the qualified name
            if (qualifiedNameSyntax.Parent is QualifiedNameSyntax) {
                return false!;
            }
        }

        if (!TryGetIdentifierType(sm, node, out var identifierType)) {
            return false;
        }

        if (identifierType.ContainingNamespace.IsGlobalNamespace) {
            // We don't need to fully qualify types in the global namespace
            return false;
        }

        globalizedIdentifier = IdentifierName(
            identifierType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        );
        return true;
    }


    private static bool TryGetIdentifierType(SemanticModel sm, SimpleNameSyntax identifierNode, out ITypeSymbol identifierType) {
        var identifier = ModelExtensions.GetSymbolInfo(sm, identifierNode).Symbol;
        if (identifier is ITypeSymbol typeIdentifier) {
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
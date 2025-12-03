using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Mumei.CodeGen.DeclarationGenerator;

public class GloballyQualifyingSyntaxRewriter(
    SemanticModel sm
) : CSharpSyntaxRewriter {
    protected SemanticModel SemanticModel = sm;

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node) {
        if (!TryGlobalizeIdentifier(SemanticModel, node, out var globalizedIdentifier)) {
            return base.VisitGenericName(node);
        }

        return globalizedIdentifier!;
    }

    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
        if (!TryGlobalizeIdentifier(SemanticModel, node, out var globalizedIdentifier)) {
            return base.VisitIdentifierName(node)!;
        }

        return globalizedIdentifier!;
    }

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node) {
        if (node.Expression is not MemberAccessExpressionSyntax) {
            return TryQualifyImplicitTargetMethodCall(node);
        }

        var invocationOperation = SemanticModel.GetOperation(node);
        if (invocationOperation is not IInvocationOperation { TargetMethod: { IsExtensionMethod: true } targetMethod }) {
            return base.VisitInvocationExpression(node);
        }

        var visited = base.VisitInvocationExpression(node);
        if (visited is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax targetMemberAccess } invocation) {
            return visited;
        }

        var methodName = targetMemberAccess.Name; // Take the existing member name to preserve type arguments
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(targetMethod!.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                methodName
            ),
            ArgumentList(SeparatedList([
                Argument(targetMemberAccess.Expression),
                ..invocation.ArgumentList.Arguments
            ]))
        ).WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
    }

    private SyntaxNode? TryQualifyImplicitTargetMethodCall(InvocationExpressionSyntax node) {
        // If the invocation is a static method imported via `using static ...` we need to qualify it
        var invocationOperation = SemanticModel.GetOperation(node);
        if (invocationOperation is not IInvocationOperation { TargetMethod: { IsStatic: true } targetMethod }) {
            return base.VisitInvocationExpression(node);
        }

        if (base.VisitInvocationExpression(node) is not InvocationExpressionSyntax visited) {
            return node;
        }

        // Ideally we would only qualify the member if it's imported via `using static ...`
        // but determining that is non-trivial so for now we always qualify static method calls
        // with their containing type
        var methodName = visited.Expression as SimpleNameSyntax ?? throw new NotSupportedException(); // Preserve type arguments
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(targetMethod!.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                methodName
            ),
            node.ArgumentList
        ).WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
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

        if (identifierType.ContainingNamespace?.IsGlobalNamespace ?? true) {
            // We don't need to fully qualify types in the global namespace
            return false;
        }

        globalizedIdentifier = IdentifierName(
            identifierType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        ).WithLeadingTrivia(node.GetLeadingTrivia()).WithTrailingTrivia(node.GetTrailingTrivia());
        return true;
    }


    private static bool TryGetIdentifierType(SemanticModel sm, SimpleNameSyntax identifierNode, out ITypeSymbol identifierType) {
        var identifier = sm.GetSymbolInfo(identifierNode).Symbol;
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;

internal sealed class SyntaxNodeFilter {
    public static bool IsInvocationOf(
        SyntaxNode node,
        string methodName,
        string immediateTargetName
    ) {
        return IsInvocationOf(node, methodName, immediateTargetName, out _, out _);
    }

    public static bool IsInvocationOf(
        SyntaxNode node,
        string methodName,
        string immediateTargetName,
        [NotNullWhen(true)] out InvocationExpressionSyntax? invocationExpression,
        [NotNullWhen(true)] out MemberAccessExpressionSyntax? memberAccessExpression
    ) {
        invocationExpression = null;
        memberAccessExpression = null;
        if (node is not InvocationExpressionSyntax invocation) {
            return false;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.Text: var actualMethodName, Expression: var targetExpression } memberAccessExpressionSyntax) {
            return false;
        }

        memberAccessExpression = memberAccessExpressionSyntax;
        if (actualMethodName != methodName) {
            return false;
        }

        if (targetExpression is not SimpleNameSyntax { Identifier.Text: var actualTargetName }) {
            if (targetExpression is not AliasQualifiedNameSyntax { Name.Identifier.Text: var actualTargetNameIdentifier }) {
                return false;
            }

            actualTargetName = actualTargetNameIdentifier;
        }

        if (actualTargetName != immediateTargetName) {
            return false;
        }

        invocationExpression = invocation;
        return true;
    }

    public static bool IsInvocationOf(
        SyntaxNode node,
        string methodName
    ) {
        if (node is not InvocationExpressionSyntax invocation) {
            return false;
        }

        if (invocation.Expression is IdentifierNameSyntax { Identifier.Text: var immediateMethodInvocationName }) {
            return immediateMethodInvocationName == methodName;
        }

        if (invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: var memberMethodInvocationName }) {
            return memberMethodInvocationName == methodName;
        }

        return false;
    }

    public static bool IsInvocationOf(
        SyntaxNode node,
        string methodName,
        out InvocationExpressionSyntax invocationExpression
    ) {
        invocationExpression = null!;
        if (node is not InvocationExpressionSyntax invocation) {
            return false;
        }

        invocationExpression = invocation;
        if (invocation.Expression is IdentifierNameSyntax { Identifier.Text: var immediateMethodInvocationName }) {
            return immediateMethodInvocationName == methodName;
        }

        if (invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: var memberMethodInvocationName }) {
            return memberMethodInvocationName == methodName;
        }

        return false;
    }
}

internal sealed class SyntaxProviderFactory {
    public static bool TryCreateInvocation(GeneratorSyntaxContext context, out InvocationIntermediateNode invocation) {
        invocation = default;
        if (context.Node is not InvocationExpressionSyntax invocationExpression) {
            return false;
        }

        if (context.SemanticModel.GetOperation(invocationExpression) is not IInvocationOperation operation) {
            return false;
        }

        invocation = new InvocationIntermediateNode(context.SemanticModel, operation, invocationExpression);
        return true;
    }
}
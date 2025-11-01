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
        return IsInvocationOf(node, methodName, immediateTargetName, out _);
    }

    public static bool IsInvocationOf(
        SyntaxNode node,
        string methodName,
        string immediateTargetName,
        [NotNullWhen(true)] out InvocationExpressionSyntax? invocationExpression
    ) {
        invocationExpression = null;
        if (node is not InvocationExpressionSyntax invocation) {
            return false;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.Text: var actualMethodName, Expression: IdentifierNameSyntax { Identifier.Text: var actualTargetName } }) {
            return false;
        }

        if (actualMethodName != methodName) {
            return false;
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
        return node is InvocationExpressionSyntax invocation && invocation.Expression is IdentifierNameSyntax { Identifier.Text: var actualMethodName } && actualMethodName == methodName;
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
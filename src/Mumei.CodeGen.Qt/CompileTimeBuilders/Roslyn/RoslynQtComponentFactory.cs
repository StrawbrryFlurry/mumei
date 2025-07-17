using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt;
using Mumei.CodeGen.Qt.Output;
using Mumei.CodeGen.Qt.Qt;

namespace Mumei.CodeGen.Playground.Roslyn;

internal readonly ref struct RoslynQtComponentFactory(
    QtCompilationScope scope
) {
    private readonly Compilation _compilation = scope.Compilation;

    public QtParameterList ParametersOf(
        IMethodSymbol method
    ) {
        var result = QtParameterList.Builder(method.Parameters.Length);

        return result;
    }

    public QtInvocation Invocation(
        InvocationExpressionSyntax invocation
    ) {
        var method = GetSymbol<IMethodSymbol>(invocation);
        var target = InvocationReplayTarget(invocation, method);
        return new QtInvocation {
            Target = target,
            Method = new QtMethodStub {
                Name = method.Name,
                IsThisCall = method.IsExtensionMethod
            },
            TypeArguments = default,
            Arguments = default
        };
    }

    private IQtInvocationTarget InvocationReplayTarget(
        InvocationExpressionSyntax invocation,
        IMethodSymbol method
    ) {
        if (method.IsStatic || method.IsExtensionMethod) {
            var declaringType = Type(method.ContainingType);
            return new QtStaticTypeInvocationTarget(declaringType);
        }

        // Missing an invocation target, has to be an implicit call to a non-static instance method.
        if (invocation.Expression is IdentifierNameSyntax or GenericNameSyntax) {
            return QtThisInvocationTarget.Instance;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) {
            throw new NotSupportedException("Found a non-static, non-instance method invocation without a valid target expression.");
        }

        return new QtInstanceInvocationTarget(memberAccess.ToString());
    }

    private QtArgumentList ReplayArguments(
        ArgumentListSyntax argumentList
    ) {
        var arguments = QtArgumentList.Builder(argumentList.Arguments.Count);
        foreach (var argument in argumentList.Arguments) { }

        return arguments;
    }

    private TSymbol GetSymbol<TSymbol>(SyntaxNode node) where TSymbol : ISymbol {
        var model = SemanticModel(node);
        var symbol = model.GetSymbolInfo(node).Symbol;
        if (symbol is not TSymbol typedSymbol) {
            throw new InvalidOperationException(
                $"Expected retrieving symbol of type {typeof(TSymbol)} from {node}, but got {symbol?.GetType().ToString() ?? "null"}"
            );
        }

        return typedSymbol;
    }

    private SemanticModel SemanticModel(SyntaxNode node) {
        return _compilation.GetSemanticModel(node.SyntaxTree);
    }

    private IQtType Type(ITypeSymbol type) {
        return new QtRoslynType(type);
    }
}

file sealed class QtRoslynType(
    ITypeSymbol typeSymbol
) : IQtType {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }
}
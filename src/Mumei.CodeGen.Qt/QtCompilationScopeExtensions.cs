using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.CodeGen.Qt;

internal static class QtCompilationScopeExtensions {
    public static IMethodSymbol GetMethodSymbol(
        this QtCompilationScope scope,
        InvocationExpressionSyntax invocation
    ) {
        var compilation = scope.Compilation;
        var sm = compilation.GetSemanticModel(invocation.SyntaxTree);
        var methodSymbol = sm.GetSymbolInfo(invocation).Symbol as IMethodSymbol
                           ?? throw new InvalidOperationException($"Could not resolve method symbol for invocation {invocation}.");

        return methodSymbol;
    }
}
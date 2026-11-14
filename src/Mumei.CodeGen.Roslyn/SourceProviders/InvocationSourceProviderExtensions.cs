using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Mumei.CodeGen.Roslyn.SourceProviders;

public sealed class Unbound;

public static class InvocationSourceProviderExtensions {
    extension(IncrementalGeneratorInitializationContext ctx) {
        public IncrementalValuesProvider<_Method> InvocationsOf(
            string declaringTypeMetadataName,
            string methodName
        ) {
            return default;
        }

        public IncrementalValuesProvider<_Method> InvocationsOf(
            string containingTypeMetadataName,
            string methodName,
            int arity
        ) {
            return default;
        }

        public IncrementalValuesProvider<_Method> InvocationsOf(Delegate methodSelector) {
            return ctx.SyntaxProvider.CreateSyntaxProvider(
                    (x, _) => x is InvocationExpressionSyntax invocation
                              && GetInvokedNameSyntax(invocation.Expression)?.Identifier.ValueText == methodSelector.Method.Name,
                    (ctx, ct) => {
                        var targetMethodSymbol = CompilationSymbolCache.TryGetTypeInCompilation(ctx.SemanticModel.Compilation, methodSelector.Method.DeclaringType);
                        var targetOperation = ctx.SemanticModel.GetOperation(ctx.Node, ct);
                        if (targetOperation is not IInvocationOperation invocation) {
                            return new Nothing();
                        }

                        return Maybe.Of(default(_Method));
                    }
                )
                .Where(x => x is Something<_Method>)
                .Select((x, _) => {
                    x.TryGetValue(out Something<_Method> m);
                    return m.Value;
                });
        }

        public IncrementalValuesProvider<_Method> InvocationsOf<T>(Func<T, Delegate> methodSelector) {
            return new IncrementalValuesProvider<_Method>();
        }
    }

    private static SimpleNameSyntax? GetInvokedNameSyntax(ExpressionSyntax expression) {
        return expression switch {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
            SimpleNameSyntax name => name,
            _ => null
        };
    }

    public sealed record _Method(
        string Name,
        _Type DeclaringType,
        Type ReturnType,
        EquatableArray<_Parameter> Parameters,
        Modifiers Modifiers
    ) { }

    public sealed record _Type { }

    public sealed record _Parameter { }
}
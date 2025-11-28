using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;
using Mumei.CodeGen.Components;
using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Roslyn.RoslynCodeProviders;

public static class SyntheticSourceProviderExtensions {
    private sealed class UserValueWithCompilationComparer<T>(IEqualityComparer<T> equalityComparer) : IEqualityComparer<UserValueWithCompilation<T>> {
        public bool Equals(UserValueWithCompilation<T> x, UserValueWithCompilation<T> y) {
            return equalityComparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(UserValueWithCompilation<T> obj) {
            return equalityComparer.GetHashCode(obj.Value);
        }
    }

    extension(IncrementalGeneratorInitializationContext ctx) {
        public IncrementalValuesProvider<UserValueWithCompilation<T>> CreateQtProvider<T>(
            [Pure] Func<SyntaxNode, CancellationToken, bool> predicate,
            [Pure] Func<GeneratorSyntaxContext, CancellationToken, T> transform,
            string? trackingName = null,
            IEqualityComparer<T>? comparer = null,
            Func<T, bool>? valueFilterPredicate = null
        ) {
            var sourceProvider = ctx.SyntaxProvider.CreateSyntaxProvider(predicate, (ctx, ct) => {
                var userResult = transform(ctx, ct);
                return new UserValueWithCompilation<T>(userResult, ctx.SemanticModel.Compilation);
            });

            if (trackingName is not null) {
                sourceProvider = sourceProvider.WithTrackingName(trackingName);
            }

            if (comparer is not null) {
                sourceProvider = sourceProvider.WithComparer(new UserValueWithCompilationComparer<T>(comparer));
            }

            if (valueFilterPredicate is not null) {
                sourceProvider = sourceProvider.Where(valueWithCompilation => valueFilterPredicate(valueWithCompilation.Value));
            }

            return sourceProvider.Select(
                (valueWithCompilation, ct) => new UserValueWithCompilation<T>(valueWithCompilation.Value, valueWithCompilation.Compilation)
            );
        }

        public void RegisterCompilationOutput(
            IncrementalValuesProvider<IncrementalCodeGenContext> contextProvider,
            Action<CodeGenerationEmitContext>? emitCode = null
        ) {
            ctx.RegisterSourceOutput(contextProvider, (spc, compilation) => {
                if (emitCode is not null) {
                    var emitContext = new CodeGenerationEmitContext(
                        compilation.Context,
                        spc.CancellationToken
                    );
                    emitCode(emitContext);
                }

                foreach (var (name, namespaces) in compilation.Context.ΦCompilerApi.EnumerateNamespacesToEmit()) {
                    using var fileTree = new SourceFileRenderTreeBuilder();

                    var text = "";
                    for (var i = 0; i < namespaces.Length; i++) {
                        var ns = namespaces[i];
                        var isLast = i == namespaces.Length - 1;
                        var fragment = compilation.Context.Synthesize<NamespaceFragment>(ns);
                        if (isLast) {
                            text = fileTree.RenderRootNode(fragment);
                        } else {
                            fileTree.Node(fragment);
                        }
                    }

                    spc.AddSource($"{name}.g", text);
                }
            });
        }

        public void RegisterCompilationOutput<T>(
            IncrementalValuesProvider<IncrementalCodeGenContext<T>> contextProvider,
            Action<CodeGenerationEmitContext, T>? emitCode = null
        ) {
            ctx.RegisterSourceOutput(contextProvider, (spc, ctx) => {
                var emitContext = new CodeGenerationEmitContext(
                    ctx.Context,
                    spc.CancellationToken
                );
                emitCode?.Invoke(emitContext, ctx.State);
            });
        }

        public void RegisterCompilationOutput<T>(
            IncrementalValuesProvider<UserValueWithCompilation<T>> contextProvider,
            Action<CodeGenerationEmitContext>? emitCode = null
        ) {
            ctx.RegisterSourceOutput(contextProvider, (spc, ctx) => {
                var context = new CSharpCodeGenerationContext();
                context.RegisterSynthesisProvider(new CompilationSynthesisProvider(ctx.Compilation));
                var emitContext = new CodeGenerationEmitContext(
                    context,
                    spc.CancellationToken
                );
                emitCode?.Invoke(emitContext);
            });
        }
    }

    extension<T>(IncrementalValuesProvider<IncrementalCodeGenContext> source) {
        public IncrementalValuesProvider<IncrementalCodeGenContext> ContinueCompile(
        ) {
            return default;
        }
    }

    extension<T>(IncrementalValuesProvider<T> source) {
        public IncrementalValuesProvider<IncrementalCodeGenContext> SelectIntoCompilation<TTo>(
            [Pure] Func<T, CancellationToken, UserValueWithCompilation<TTo>> selector
        ) {
            return default;
        }
    }

    extension<T>(IncrementalValuesProvider<UserValueWithCompilation<T>> source) {
        public IncrementalValuesProvider<IncrementalCodeGenContext> Combine<TOther>() {
            // var valueWithCompilation = source.Select((vwc, ct) => new UserValueWithCompilation<TTo>(default!, vwc.Compilation));
            return default;
        }

        public IncrementalValuesProvider<IncrementalCodeGenContext<TTo>> IncrementalCompile<TTo>(
            [Pure] Func<ICodeGenerationContext, T, TTo> compileSelector
        ) {
            // var valueWithCompilation = source.Select(selector);
            return default;
        }

        public IncrementalValuesProvider<IncrementalCodeGenContext> IncrementalCompile(
            [Pure] Action<ICodeGenerationContext, T, CancellationToken> compileSelector
        ) {
            // var valueWithCompilation = source.Select(selector);
            return source.Select((valueWithCompilation, ct) => {
                var context = new CSharpCodeGenerationContext();
                context.RegisterSynthesisProvider(new CompilationSynthesisProvider(valueWithCompilation.Compilation));
                compileSelector(context, valueWithCompilation.Value, ct);
                return new IncrementalCodeGenContext(context, [valueWithCompilation.Value]);
            });
        }
    }
}

public readonly struct UserValueWithCompilation<T>(T userValue, Compilation compilation) : IEquatable<UserValueWithCompilation<T>> {
    public T Value { get; } = userValue;
    public Compilation Compilation { get; } = compilation;

    public bool Equals(UserValueWithCompilation<T> other) {
        if (Value is null && other.Value is null) {
            return true;
        }

        return Value?.Equals(other.Value) ?? false;
    }

    public override bool Equals(object? obj) {
        return obj is UserValueWithCompilation<T> other && Equals(other);
    }

    public override int GetHashCode() {
        return Value?.GetHashCode() ?? 0;
    }
}

public readonly struct CodeGenerationEmitContext(ICodeGenerationContext context, CancellationToken ct) {
    public ICodeGenerationContext Context { get; } = context;
    public CancellationToken CancellationToken { get; } = ct;

    public void Emit(string hintName, ISyntheticNamespace ns) {
        Context.Emit(hintName, ns);
    }
}
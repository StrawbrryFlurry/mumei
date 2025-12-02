using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext : ICodeGenerationContext {
    private readonly Dictionary<Type, object> _synthesisProviders = new();

    public CompilationUnitFragment SynthesizeCompilationUnit(
        ImmutableArray<ISyntheticDeclaration> declarations,
        ISyntheticIdentifierScopeProvider identifierScopeProvider
    ) {
        var constructedNamespaces = new ArrayBuilder<NamespaceOrGlobalScopeFragment>();
        var ctx = new CSharpCompilationUnitContext(
            this,
            identifierScopeProvider
        );

        foreach (var declaration in declarations) {
            var fragment = ctx.Synthesize<NamespaceOrGlobalScopeFragment>(declaration);
            constructedNamespaces.Add(fragment);
        }

        var updatedCompilationUnit = ctx.ApplyCodeGenerationFeatures(new CompilationUnitFragment(
            TriviaFragment.Empty,
            constructedNamespaces.ToImmutableArrayAndFree(),
            TriviaFragment.Empty
        ));
        return updatedCompilationUnit;
    }

    public void RegisterContextProvider<TSynthesizer>(TSynthesizer synthesizer) where TSynthesizer : ICodeGenerationContextProvider {
        _synthesisProviders[typeof(TSynthesizer)] = synthesizer;
    }

    public TProvider GetContextProvider<TProvider>() where TProvider : ICodeGenerationContextProvider {
        if (_synthesisProviders.TryGetValue(typeof(TProvider), out var provider)) {
            return (TProvider) provider;
        }

        throw new InvalidOperationException($"No synthesis provider of type {typeof(TProvider)} has been registered.");
    }
}

internal sealed class CSharpCompilationUnitContext(
    ICodeGenerationContext codeGenContext,
    ISyntheticIdentifierScopeProvider identifierScopeProvider
) : ICompilationUnitContext {
    public ICodeGenerationContext CodeGenContext => codeGenContext;
    private HashSet<ICompilationUnitFeature>? _sharedLocalDeclarations;

    public CompilationUnitFragment ApplyCodeGenerationFeatures(CompilationUnitFragment currentUnit) {
        if (_sharedLocalDeclarations is null) {
            return currentUnit;
        }

        var updatedUnit = currentUnit;
        foreach (var feature in _sharedLocalDeclarations) {
            updatedUnit = feature.Implement(this, updatedUnit);
        }

        return updatedUnit;
    }

    public void AddSharedLocalCompilationUnitFeature(ICompilationUnitFeature feature) {
        _sharedLocalDeclarations ??= [];
        _sharedLocalDeclarations.Add(feature);
    }

    public T Synthesize<T>(object? constructable, T? defaultValue = default) {
        if (constructable is ISyntheticConstructable<T> c) {
            return c.Construct(this);
        }

        if (!defaultValue?.Equals(default(T)) ?? true) {
            return defaultValue!;
        }

        throw new NotSupportedException();
    }

    public T? SynthesizeOptional<T>(object? constructable) {
        if (constructable is ISyntheticConstructable<T> c) {
            return c.Construct(this);
        }

        return default;
    }

    public ISyntheticIdentifierScope GetDeclarationScope(ISyntheticDeclaration scope) {
        return identifierScopeProvider.GetDeclarationScope(scope);
    }
}
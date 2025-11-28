using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext : ICodeGenerationContext {
    private readonly Dictionary<string, Dictionary<string, ISyntheticNamespace>> _namespacesToEmit = new();
    private readonly Dictionary<Type, object> _synthesisProviders = new();

    public CompilationUnitFragment SynthesizeCompilationUnit(ImmutableArray<ISyntheticNamespace> namespaces) {
        var constructedNamespaces = new ArrayBuilder<NamespaceFragment>();
        var ctx = new CSharpCompilationUnitContext(this);
        foreach (var ns in namespaces) {
            var fragment = ctx.Synthesize<NamespaceFragment>(ns);
            constructedNamespaces.Add(fragment);
        }

        var updatedCompilationUnit = ctx.ApplyCodeGenerationFeatures(new CompilationUnitFragment(
            TriviaFragment.Empty,
            constructedNamespaces.ToImmutableArrayAndFree(),
            TriviaFragment.Empty
        ));
        return updatedCompilationUnit;
    }

    public void Emit(string hintName, ISyntheticNamespace ns) {
        if (!_namespacesToEmit.TryGetValue(hintName, out var existingNamespaceMap)) {
            _namespacesToEmit[hintName] = new Dictionary<string, ISyntheticNamespace> {
                [ns.FullyQualifiedName] = ns
            };
            return;
        }

        if (!existingNamespaceMap.TryGetValue(ns.FullyQualifiedName, out var existingMatchingNamespace)) {
            existingNamespaceMap[ns.FullyQualifiedName] = ns;
            return;
        }

        foreach (var member in ns.Members) {
            existingMatchingNamespace.WithMember(member);
        }
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

internal sealed class CSharpCompilationUnitContext(ICodeGenerationContext codeGenContext) : ICompilationUnitContext {
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

    public void AddSharedLocalDeclaration(ICompilationUnitFeature feature) {
        _sharedLocalDeclarations ??= [];
        _sharedLocalDeclarations.Add(feature);
    }

    public T? Synthesize<T>(object? constructable, T? defaultValue = default) {
        if (constructable is ISyntheticConstructable<T> c) {
            return c.Construct(this);
        }

        if (!defaultValue?.Equals(default(T)) ?? true) {
            return defaultValue;
        }

        throw new NotSupportedException();
    }

    public T? SynthesizeOptional<T>(object? constructable) {
        if (constructable is ISyntheticConstructable<T> c) {
            return c.Construct(this);
        }

        return default;
    }
}
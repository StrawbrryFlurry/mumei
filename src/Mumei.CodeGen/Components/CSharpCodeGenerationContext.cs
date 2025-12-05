using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext : ICodeGenerationContext {
    private readonly Dictionary<Type, object> _synthesisProviders = new();

    private ISyntheticNamespaceBuilder _globalNamespaceBuilder;
    public ISyntheticNamespaceBuilder GlobalNamespace => _globalNamespaceBuilder;

    public CSharpCodeGenerationContext() {
        ΦCompilerApi = new CompilerApiImpl(this);
        _globalNamespaceBuilder = new GlobalSyntheticNamespace(ΦCompilerApi);
    }

    public CompilationUnitFragment SynthesizeCompilationUnit(
        ImmutableArray<ISyntheticDeclaration> declarations,
        ISyntheticIdentifierScopeProvider? identifierScopeProvider
    ) {
        var constructedNamespaces = new ArrayBuilder<NamespaceOrGlobalScopeFragment>();
        var ctx = new CSharpCompilationUnitContext(
            this,
            identifierScopeProvider ?? NoOpSyntheticIdentifierScopeProvider.Instance
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

    public bool Equals(ICodeGenerationContext? other) {
        if (other is null) { return false; }
        if (ReferenceEquals(this, other)) { return true; }
        return GlobalNamespacesAreSame(GlobalNamespace, other.GlobalNamespace);
    }

    private bool GlobalNamespacesAreSame(ISyntheticNamespace a, ISyntheticNamespace b) {
        var resultA = SynthesizeCompilationUnit(
            a.Members,
            NoOpSyntheticIdentifierScopeProvider.Instance
        );

        var resultB = SynthesizeCompilationUnit(
            b.Members,
            NoOpSyntheticIdentifierScopeProvider.Instance
        );

        var emitA = new SourceFileRenderTreeBuilder();
        var emitB = new SourceFileRenderTreeBuilder();

        emitA.RenderRootNode(resultA);
        emitB.RenderRootNode(resultB);

        return emitA.ToString() == emitB.ToString();
    }

    public override bool Equals(object? obj) {
        return ReferenceEquals(this, obj) || obj is CSharpCodeGenerationContext other && Equals(other);
    }

    public override int GetHashCode() {
        return 0;
    }

    private sealed class NoOpSyntheticIdentifierScopeProvider : ISyntheticIdentifierScopeProvider {
        public static readonly NoOpSyntheticIdentifierScopeProvider Instance = new();

        public ISyntheticIdentifierScope GetDeclarationScope(ISyntheticDeclaration scope) {
            return NoOpSyntheticIdentifierScope.Instance;
        }

        private sealed class NoOpSyntheticIdentifierScope : ISyntheticIdentifierScope {
            public static readonly NoOpSyntheticIdentifierScope Instance = new();

            public string MakeUnique(string baseName) {
                return baseName;
            }
        }
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

        throw new NotSupportedException($"Cannot synthesize the requested type {typeof(T).FullName} from {constructable?.GetType().FullName ?? "null"}.");
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
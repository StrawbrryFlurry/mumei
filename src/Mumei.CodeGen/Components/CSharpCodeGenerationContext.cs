using System.Collections.Immutable;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen.Components;

internal sealed class CSharpCodeGenerationContext : ICodeGenerationContext {
    private readonly Dictionary<string, Dictionary<string, ISyntheticNamespace>> _namespacesToEmit = new();

    public ISyntheticCodeBlock Block(RenderFragment renderBlock) {
        return new QtSyntheticRenderCodeBlock(renderBlock);
    }

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(string name) {
        return new QtSyntheticClassBuilder<CompileTimeUnknown>(name, this);
    }

    public ISyntheticNamespace Namespace(params ReadOnlySpan<string> namespaceSegments) {
        throw new NotImplementedException();
    }

    public T? Synthesize<T>(object? constructable, T? defaultValue = default) {
        if (constructable is ISyntheticConstructable<T> c) {
            var ctx = new CSharpCompilationUnitContext(this);
            return c.Construct(ctx);
        }

        if (!defaultValue?.Equals(default(T)) ?? true) {
            return defaultValue;
        }

        throw new NotSupportedException();
    }

    public T? SynthesizeOptional<T>(object? constructable) {
        if (constructable is ISyntheticConstructable<T> c) {
            var ctx = new CSharpCompilationUnitContext(this);
            return c.Construct(ctx);
        }

        return default;
    }

    public void Emit(string hintName, ISyntheticNamespace toEmit) {
        throw new NotImplementedException();
    }

    public void RegisterSynthesisProvider<TSynthesizer>(TSynthesizer synthesizer) where TSynthesizer : ISynthesisProvider {
        throw new NotImplementedException();
    }

    public TProvider GetSynthesisProvider<TProvider>() where TProvider : ISynthesisProvider {
        throw new NotImplementedException();
    }

    public ICodeGenerationContext.IΦInternalCompilerApi ΦCompilerApi => field ??= new CompilerApiImpl(this);

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(string name, Action<TClassDefinition> inputBinder) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
        throw new NotImplementedException();
    }

    public void TrackForEmission(string hintName, ISyntheticNamespace ns) {
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

    private sealed class CompilerApiImpl(CSharpCodeGenerationContext context) : ICodeGenerationContext.IΦInternalCompilerApi {
        private int _internalTrackingId = 0;

        public string MakeArbitraryUniqueName(string name) {
            return $"{name}__{_internalTrackingId++}";
        }

        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name) {
            return new QtSyntheticClassBuilder<TClassDefinition>(name, context);
        }

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
            return classBuilder;
        }

        private UniqueNameGeneratorComponent? _uniqueNameGenerator;

        public string NextId() {
            return (_uniqueNameGenerator ??= new UniqueNameGeneratorComponent()).MakeUnique("");
        }

        public ImmutableArray<(string TrackingName, ImmutableArray<ISyntheticNamespace> Namespaces)> EnumerateNamespacesToEmit() {
            return context._namespacesToEmit.Select(x => (x.Key, x.Value.Values.ToImmutableArray())).ToImmutableArray();
        }
    }
}

internal sealed class CSharpCompilationUnitContext(ICodeGenerationContext codeGenContext) : ICompilationUnitContext {
    public ICodeGenerationContext CodeGenContext => codeGenContext;

    public T? Synthesize<T>(object? constructable, T? defaultValue = default) {
        return CodeGenContext.Synthesize(constructable, defaultValue);
    }

    public T? SynthesizeOptional<T>(object? constructable) {
        return CodeGenContext.SynthesizeOptional<T>(constructable);
    }
}
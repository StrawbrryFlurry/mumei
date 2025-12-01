using System.Collections.Concurrent;
using Microsoft.Interop;

namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext {
    public ICodeGenerationContext.IΦInternalCompilerApi ΦCompilerApi => field ??= new CompilerApiImpl(this);

    private sealed class CompilerApiImpl(CSharpCodeGenerationContext context) : ICodeGenerationContext.IΦInternalCompilerApi {
        public ICodeGenerationIdentifierProvider IdentifierProvider { get; } = new CSharpCodeGenerationIdentifierProvider();

        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name) {
            return new QtSyntheticClassBuilder<TClassDefinition>(new ConstantSyntheticIdentifier(name), context);
        }

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
            return classBuilder;
        }

        public ImmutableArray<(ISyntheticIdentifier TrackingName, ImmutableArray<ISyntheticNamespace> Namespaces)> EnumerateNamespacesToEmit() {
            return context._namespacesToEmit.Select(x => (x.Key, x.Value.Values.ToImmutableArray())).ToImmutableArray();
        }
    }
}

internal sealed class CSharpCodeGenerationIdentifierProvider : ICodeGenerationIdentifierProvider {
    public IdentifierScope GlobalScope { get; } = IdentifierScope.Global;

    private readonly ConcurrentDictionary<ScopeIdEntryLookup, (int ParentId, int ScopeId)> _namedScopeIds = new();
    private int _currentScopeId = 0;

    public IdentifierScope CreateScope(IdentifierScope parentScope, string name) {
        return GetOrAddNamedScope(parentScope, name);
    }

    public IdentifierScope CreateScope(string name) {
        return GetOrAddNamedScope(GlobalScope, name);
    }

    private IdentifierScope GetOrAddNamedScope(IdentifierScope parent, string name) {
        var entry = new ScopeIdEntryLookup(parent, name);
        var scopeId = _namedScopeIds.GetOrAdd(
            entry,
            e => (e.ParentScope.ScopeId, Interlocked.Increment(ref _currentScopeId))
        );

        return new IdentifierScope(scopeId.ScopeId);
    }

    internal CSharpCodeGenerationIdentifierProvider Merge(CSharpCodeGenerationIdentifierProvider other) {
        // Find a way to re-align the scope ids if we have two competing providers
        // being emitted into the same context.

        return this;
    }

    private readonly struct ScopeIdEntryLookup(
        IdentifierScope parentScope,
        string name
    ) : IEquatable<ScopeIdEntryLookup> {
        private readonly string _name = name;
        public IdentifierScope ParentScope => parentScope;

        public bool Equals(ScopeIdEntryLookup other) {
            return _name == other._name && parentScope.Equals(other.ParentScope);
        }

        public override bool Equals(object? obj) {
            return obj is ScopeIdEntryLookup other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_name, parentScope);
        }
    }
}
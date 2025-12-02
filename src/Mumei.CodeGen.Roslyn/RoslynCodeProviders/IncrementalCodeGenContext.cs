using Mumei.CodeGen.Components;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Roslyn.RoslynCodeProviders;

// TODO: Since we require the incremental compilation selectors to be pure functions
// we should add some validation to ensure that the output always matches the input dependencies.
// We could simply just run the selector multiple times with the same inputs in development builds
// to ensure that the outputs are consistent.
public readonly struct IncrementalCodeGenContext(
    ICodeGenerationContext context,
    EquatableImmutableArray<object> dependencies,
    ISyntheticIdentifierScopeProvider? sharedIdentifierScopeProvider = null
) : IEquatable<IncrementalCodeGenContext> {
    public ICodeGenerationContext Context { get; } = context;
    public ISyntheticIdentifierScopeProvider? SharedIdentifierScopeProvider { get; } = sharedIdentifierScopeProvider;

    // Since we can assume that any compilation created from the same inputs will always lead to the same output,
    // we can use the dependencies to determine equality.
    private readonly EquatableImmutableArray<object> _dependencies = dependencies;

    public bool Equals(IncrementalCodeGenContext other) {
        return _dependencies.Equals(other._dependencies);
    }

    public override bool Equals(object? obj) {
        return obj is IncrementalCodeGenContext other && Equals(other);
    }

    public override int GetHashCode() {
        return _dependencies.GetHashCode();
    }
}

public readonly struct IncrementalCodeGenContext<TState>(
    ICodeGenerationContext context,
    TState state,
    EquatableImmutableArray<object> dependencies
) : IEquatable<IncrementalCodeGenContext<TState>> {
    public ICodeGenerationContext Context { get; } = context;
    public TState State { get; } = state;

    private readonly EquatableImmutableArray<object> _dependencies = dependencies;

    public bool Equals(IncrementalCodeGenContext<TState> other) {
        return _dependencies.Equals(other._dependencies);
    }

    public override bool Equals(object? obj) {
        return obj is IncrementalCodeGenContext<TState> other && Equals(other);
    }

    public override int GetHashCode() {
        return _dependencies.GetHashCode();
    }
}
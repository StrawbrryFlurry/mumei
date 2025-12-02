using Mumei.CodeGen.Components;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Roslyn.RoslynCodeProviders;

// TODO: Since we require the incremental compilation selectors to be pure functions
// we should add some validation to ensure that the output always matches the input dependencies.
// We could simply just run the selector multiple times with the same inputs in development builds
// to ensure that the outputs are consistent.
public readonly struct IncrementalCodeGenContext(
    ICodeGenerationContext context,
    ISyntheticIdentifierScopeProvider? sharedIdentifierScopeProvider = null
) : IEquatable<IncrementalCodeGenContext> {
    public ICodeGenerationContext Context { get; } = context;
    public ISyntheticIdentifierScopeProvider? SharedIdentifierScopeProvider { get; } = sharedIdentifierScopeProvider;

    public bool Equals(IncrementalCodeGenContext other) {
        // TODO: We should ideally compare the global namespace of one compilation with another
        // to ensure changes that happen outside of our dependencies are also detected. E.g. changing a method signature
        // from a syntax node that resides in that method declaration and is part of the dependencies.
        return context.Equals(other.Context);
    }

    public override bool Equals(object? obj) {
        return obj is IncrementalCodeGenContext other && Equals(other);
    }

    public override int GetHashCode() {
        return context.GetHashCode();
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
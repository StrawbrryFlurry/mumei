using Mumei.CodeGen.Components;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.RoslynCodeProviders;

// TODO: Since we require the incremental compilation selectors to be pure functions
// we should add some validation to ensure that the output always matches the input dependencies.
// We could simply just run the selector multiple times with the same inputs in development builds
// to ensure that the outputs are consistent.
public readonly struct IncrementalSyntheticCompilation(
    ISyntheticCompilation compilation,
    EquatableImmutableArray<object> dependencies
) : IEquatable<IncrementalSyntheticCompilation> {
    public ISyntheticCompilation Compilation { get; } = compilation;

    // Since we can assume that any compilation created from the same inputs will always lead to the same output,
    // we can use the dependencies to determine equality.
    private readonly EquatableImmutableArray<object> _dependencies = dependencies;

    public bool Equals(IncrementalSyntheticCompilation other) {
        return _dependencies.Equals(other._dependencies);
    }

    public override bool Equals(object? obj) {
        return obj is IncrementalSyntheticCompilation other && Equals(other);
    }

    public override int GetHashCode() {
        return _dependencies.GetHashCode();
    }
}

public readonly struct IncrementalSyntheticCompilation<TContext>(
    ISyntheticCompilation compilation,
    TContext context,
    EquatableImmutableArray<object> dependencies
) : IEquatable<IncrementalSyntheticCompilation<TContext>> {
    public ISyntheticCompilation Compilation { get; } = compilation;
    public TContext Context { get; } = context;

    private readonly EquatableImmutableArray<object> _dependencies = dependencies;

    public bool Equals(IncrementalSyntheticCompilation<TContext> other) {
        return _dependencies.Equals(other._dependencies);
    }

    public override bool Equals(object? obj) {
        return obj is IncrementalSyntheticCompilation<TContext> other && Equals(other);
    }

    public override int GetHashCode() {
        return _dependencies.GetHashCode();
    }
}
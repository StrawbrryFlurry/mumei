using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mumei.Roslyn;

[CollectionBuilder(typeof(ΦEquatableImmutableArrayBuilder), nameof(ΦEquatableImmutableArrayBuilder.Create))]
public readonly struct EquatableImmutableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableImmutableArray<T>>, IEnumerable<T> {
    public ImmutableArray<T> Array { get; } = array;

    public static implicit operator EquatableImmutableArray<T>(ImmutableArray<T> array) {
        return new EquatableImmutableArray<T>(array);
    }

    public static implicit operator ImmutableArray<T>(EquatableImmutableArray<T> equatableArray) {
        return equatableArray.Array;
    }

    public bool Equals(EquatableImmutableArray<T> other) {
        if (Array.Length != other.Array.Length) {
            return false;
        }

        for (var i = 0; i < Array.Length; i++) {
            var ourElement = Array[i];
            if (ourElement is null) {
                if (other.Array[i] is not null) {
                    return false;
                }

                continue;
            }

            if (!ourElement.Equals(other.Array[i])) {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) {
        return obj is EquatableImmutableArray<T> other && Equals(other);
    }

    public override int GetHashCode() {
        return Array.GetHashCode();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return Array.GetEnumeratorInterfaceImplementation();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return Array.GetEnumeratorInterfaceImplementation();
    }
}

public static class ΦEquatableImmutableArrayBuilder {
    public static EquatableImmutableArray<TElement> Create<TElement>(ReadOnlySpan<TElement> elements) {
        return new EquatableImmutableArray<TElement>([
            ..elements
        ]);
    }
}
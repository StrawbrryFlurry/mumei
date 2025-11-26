using System.Collections;
using System.Collections.Immutable;

namespace Mumei.Roslyn;

public readonly struct EquatableImmutableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableImmutableArray<T>>, IEnumerable {
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

    IEnumerator IEnumerable.GetEnumerator() {
        return Array.GetEnumeratorInterfaceImplementation();
    }
}
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mumei.CodeGen;

[CollectionBuilder(typeof(EquatableArrayBuilder), nameof(EquatableArrayBuilder.Create))]
public readonly struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>>, IEnumerable<T> {
    public ImmutableArray<T> Array { get; } = array;

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) {
        return new EquatableArray<T>(array);
    }

    public static implicit operator ImmutableArray<T>(EquatableArray<T> equatableArray) {
        return equatableArray.Array;
    }

    public bool Equals(EquatableArray<T> other) {
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
        return obj is EquatableArray<T> other && Equals(other);
    }

    public override int GetHashCode() {
        return Array.GetHashCode();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return ((IEnumerable<T>)this).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)this).GetEnumerator();
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class EquatableArrayBuilder {
    public static EquatableArray<TElement> Create<TElement>(ReadOnlySpan<TElement> elements) {
        return new EquatableArray<TElement>([
            .. elements
        ]);
    }
}
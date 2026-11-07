using System.Collections;
using System.Runtime.CompilerServices;

namespace Mumei.CodeGen;

/// <summary>
/// An immutable array with value equality, safe to use in incremental generator models.
/// Supports collection expressions: <c>EquatableArray&lt;string&gt; names = ["a", "b"];</c>
/// </summary>
[CollectionBuilder(typeof(EquatableArray), nameof(EquatableArray.Create))]
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T> {
    public static readonly EquatableArray<T> Empty = new([]);

    private readonly T[]? _items;

    public EquatableArray(T[] items) {
        _items = items;
    }

    public int Count => _items?.Length ?? 0;
    public int Length => Count;
    public bool IsEmpty => Count == 0;

    public T this[int index] => (_items ?? [])[index];

    public ReadOnlySpan<T> AsSpan() {
        return _items;
    }

    public bool Equals(EquatableArray<T> other) {
        return AsSpan().SequenceEqual(other.AsSpan(), EqualityComparer<T>.Default);
    }

    public override bool Equals(object? obj) {
        return obj is EquatableArray<T> other && Equals(other);
    }

    public override int GetHashCode() {
        var hash = new HashCode();
        foreach (var item in AsSpan()) {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) {
        return left.Equals(right);
    }

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) {
        return !left.Equals(right);
    }

    public static implicit operator EquatableArray<T>(T[] items) {
        return new EquatableArray<T>(items);
    }

    public static implicit operator EquatableArray<T>(ImmutableArray<T> items) {
        return new EquatableArray<T>(items.IsDefault ? [] : [..items]);
    }

    public ReadOnlySpan<T>.Enumerator GetEnumerator() {
        return AsSpan().GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return ((IEnumerable<T>)(_items ?? [])).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return (_items ?? []).GetEnumerator();
    }

    public override string ToString() {
        return $"[{string.Join(", ", _items ?? [])}]";
    }
}

public static class EquatableArray {
    public static EquatableArray<T> Create<T>(ReadOnlySpan<T> items) {
        return new EquatableArray<T>(items.ToArray());
    }

    public static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> items) {
        return new EquatableArray<T>(items.ToArray());
    }
}

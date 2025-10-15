using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Qt.Containers;

/// <summary>
/// Similar to <see cref="ImmutableArray{T}"/>, but allows for adding elements
/// in a less safe, but more memory efficient way.
/// </summary>
/// <typeparam name="TElement"></typeparam>
[CollectionBuilder(typeof(λImmutableComponentCollectionFactory), nameof(λImmutableComponentCollectionFactory.Create))]
public readonly struct QtCollection<TElement> : IReadOnlyCollection<TElement>, IQtMemoryAccessor<TElement>, IDisposable {
    internal const int InitialCapacity = 4;

    // Null for "default" instances
    private readonly TElement[]? _elements = [];
    private readonly bool _borrowed = false;

    public int Count { get; } = 0;
    public bool IsEmpty => Count == 0;

    public Memory<TElement> Memory {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements.AsMemory(0, Count);
    }

    public Span<TElement> Span {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements.AsSpan(0, Count);
    }

    public static QtCollection<TElement> Empty => default;

    public QtCollection() { }

    private QtCollection(TElement[] elements, int count, bool borrowed = false) {
        Count = count;
        _elements = elements;
        _borrowed = borrowed;
    }

    /// <summary>
    /// Creates a new immutable component collection from the given elements, taking ownership of the array.
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="length">The length of the actual elements to take from <paramref name="elements"/> or null if all elements should be used</param>
    /// <param name="borrowed">Whether the provided array was borrowed from the shared array pool</param>
    /// <returns></returns>
    public static QtCollection<TElement> UnsafeCreateFrom(TElement[] elements, int? length = null, bool borrowed = false) {
        var len = length ?? elements.Length;
        return new QtCollection<TElement>(elements, len, borrowed);
    }

    public static QtCollection<TElement> CreateFrom(ReadOnlySpan<TElement> elements) {
        if (elements.IsEmpty) {
            return Empty;
        }

        var bufferSize = Math.Max(InitialCapacity, elements.Length);
        var buffer = new TElement[bufferSize];
        elements.CopyTo(buffer);
        return UnsafeCreateFrom(buffer, elements.Length);
    }

    public static QtCollection<TElement> Create(int length) {
        return UnsafeCreateFrom(new TElement[length], 0);
    }

    public TElement this[int index] {
        // Use span to throw on out-of-bounds exceptions on default instances
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_elements ?? [])[index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal set => (_elements ?? [])[index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QtCollection<TElement> Add(in TElement element) {
        var idx = Count;
        var coll = EnsureCapacity(1);
        // We know this is never null after EnsureCapacity - MemberNotNull won't work here because coll may not be "this"
        coll._elements![idx] = element;
        return coll;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QtCollection<TElement> AddRange(in ReadOnlySpan<TElement> elements) {
        if (elements.IsEmpty) {
            return this;
        }

        var idx = Count;
        var coll = EnsureCapacity(elements.Length);
        elements.CopyTo(coll._elements.AsSpan(idx));
        return coll;
    }

    [SkipLocalsInit]
    public QtCollection<TElement> EnsureCapacity(int newElementCount) {
        var elements = _elements;
        var currentCount = Count;
        var requiredCapacity = currentCount + newElementCount;

        if (elements is null || elements.Length == 0) {
            var initialSize = Math.Max(InitialCapacity, newElementCount);
            var initialElements = new TElement[initialSize];
            return new QtCollection<TElement>(initialElements, newElementCount);
        }

        if (requiredCapacity <= elements.Length) {
            return new QtCollection<TElement>(elements, requiredCapacity, _borrowed);
        }

        var newSize = Math.Max(elements.Length * 2, requiredCapacity);
        var newElements = new TElement[newSize];
        elements.CopyTo(newElements, 0);
        if (_borrowed) {
            ArrayPool<TElement>.Shared.Return(elements);
        }

        return new QtCollection<TElement>(newElements, requiredCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<TElement>.Enumerator GetEnumerator() {
        return _elements.AsSpan(0, Count).GetEnumerator();
    }

    IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() {
        Debug.Assert(false, "We never want to use this overload");
        return (_elements ?? []).AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<TElement>) this).GetEnumerator();
    }

    public void Dispose() {
        if (_borrowed) {
            ArrayPool<TElement>.Shared.Return(_elements);
        }
    }

    public static implicit operator Span<TElement>(QtCollection<TElement> collection) {
        return collection.Span;
    }

    public static implicit operator ReadOnlySpan<TElement>(QtCollection<TElement> collection) {
        return collection.Span;
    }

    public static implicit operator Memory<TElement>(QtCollection<TElement> collection) {
        return collection.Memory;
    }
}

public static class QtCollectionExtensions {
    public static QtCollection<TTo> Select<T, TTo>(this in QtCollection<T> collection, Func<T, TTo> mapper) {
        var destination = QtCollection<TTo>.Create(collection.Count);
        foreach (var item in collection) {
            destination = destination.Add(mapper(item));
        }

        return destination;
    }
}

// Cannot be file scoped.
// ReSharper disable once InconsistentNaming
public static class λImmutableComponentCollectionFactory {
    public static QtCollection<TElement> Create<TElement>(ReadOnlySpan<TElement> elements) {
        var elementLength = elements.Length;
        if (elementLength == 0) {
            return QtCollection<TElement>.Empty;
        }

        var bufferSize = Math.Max(QtCollection<TElement>.InitialCapacity, elementLength);
        var buffer = new TElement[bufferSize];
        elements.CopyTo(buffer);
        return QtCollection<TElement>.UnsafeCreateFrom(buffer, elementLength);
    }
}
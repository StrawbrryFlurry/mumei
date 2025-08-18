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
public readonly struct ImmutableComponentCollection<TElement> : IReadOnlyCollection<TElement>, IQtMemoryAccessor<TElement>, IDisposable {
    internal const int InitialCapacity = 4;

    private readonly TElement[] _elements = [];
    private readonly bool _borrowed = false;

    public int Count { get; } = 0;

    public Memory<TElement> Memory {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements.AsMemory();
    }

    public Span<TElement> Span {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements.AsSpan();
    }

    public static ImmutableComponentCollection<TElement> Empty => default;

    public ImmutableComponentCollection() { }

    private ImmutableComponentCollection(TElement[] elements, int count, bool borrowed = false) {
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
    public static ImmutableComponentCollection<TElement> UnsafeCreateFrom(TElement[] elements, int? length = null, bool borrowed = false) {
        var len = length ?? elements.Length;
        return new ImmutableComponentCollection<TElement>(elements, len, borrowed);
    }

    public static ImmutableComponentCollection<TElement> CreateFrom(ReadOnlySpan<TElement> elements) {
        if (elements.IsEmpty) {
            return Empty;
        }

        var bufferSize = Math.Max(InitialCapacity, elements.Length);
        var buffer = new TElement[bufferSize];
        elements.CopyTo(buffer);
        return UnsafeCreateFrom(buffer, elements.Length);
    }

    public TElement this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableComponentCollection<TElement> Add(in TElement element) {
        var coll = EnsureCapacity(1);
        coll._elements[coll.Count] = element;
        return coll;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableComponentCollection<TElement> AddRange(in ReadOnlySpan<TElement> elements) {
        if (elements.IsEmpty) {
            return this;
        }

        var coll = EnsureCapacity(elements.Length);
        elements.CopyTo(coll._elements.AsSpan(coll.Count));
        return coll;
    }

    [SkipLocalsInit]
    public ImmutableComponentCollection<TElement> EnsureCapacity(int newElementCount) {
        var elements = _elements;
        var currentCount = Count;
        var requiredCapacity = currentCount + newElementCount;
        if (requiredCapacity <= elements.Length) {
            return this;
        }

        if (elements.Length == 0) {
            var initialSize = Math.Max(InitialCapacity, newElementCount);
            var initialElements = new TElement[initialSize];
            return new ImmutableComponentCollection<TElement>(initialElements, newElementCount);
        }

        var newSize = Math.Max(elements.Length * 2, requiredCapacity);
        var newElements = new TElement[newSize];
        elements.CopyTo(newElements, 0);
        if (_borrowed) {
            ArrayPool<TElement>.Shared.Return(elements);
        }

        return new ImmutableComponentCollection<TElement>(newElements, requiredCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<TElement>.Enumerator GetEnumerator() {
        return _elements.AsSpan(0, Count).GetEnumerator();
    }

    IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() {
        Debug.Assert(false, "We never want to use this overload");
        return _elements.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<TElement>) this).GetEnumerator();
    }

    public void Dispose() {
        if (_borrowed) {
            ArrayPool<TElement>.Shared.Return(_elements);
        }
    }
}

// Cannot be file scoped.
// ReSharper disable once InconsistentNaming
public static class λImmutableComponentCollectionFactory {
    public static ImmutableComponentCollection<TElement> Create<TElement>(ReadOnlySpan<TElement> elements) {
        var elementLength = elements.Length;
        if (elementLength == 0) {
            return ImmutableComponentCollection<TElement>.Empty;
        }

        var bufferSize = Math.Max(ImmutableComponentCollection<TElement>.InitialCapacity, elementLength);
        var buffer = new TElement[bufferSize];
        elements.CopyTo(buffer);
        return ImmutableComponentCollection<TElement>.UnsafeCreateFrom(buffer, elementLength);
    }
}
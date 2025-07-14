using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Mumei.Roslyn;

public ref partial struct ArrayBuilder<TElement> {
    private const int DefaultCapacity = 4;

    private Span<TElement> _elements;
    private TElement[] _rentedArray;
    private int _count = 0;
    private int _capacity = 0;

    private Span<TElement> SlicedElements {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _elements[.._count];
    }

    public static ArrayBuilder<TElement> Empty => new();

    public ArrayBuilder() {
        _elements = _rentedArray = Array.Empty<TElement>();
    }

    /// <summary>
    /// Consumers may use the <see cref="CreateWithApproximateSize(int)"/> method to create a builder
    /// with an initial capacity which ensures that the builder does not rent an array with a
    /// suboptimal size.
    /// </summary>
    /// <param name="initialCapacity"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal ArrayBuilder(int initialCapacity) {
        if (initialCapacity < 0) {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }

        SetBackingArray(AllocateArray(initialCapacity));
        _capacity = initialCapacity;
    }

    public static ArrayBuilder<TElement> CreateWithApproximateSize(int sizeHint) {
        return new ArrayBuilder<TElement>(GetCapacityForSize(sizeHint));
    }

    public static ArrayBuilder<TElement> CreateWithApproximateSize(double sizeHint) {
        return new ArrayBuilder<TElement>(GetCapacityForSize((int)sizeHint));
    }

    private static int GetCapacityForSize(int count) {
        if (count < 0) {
            return 0;
        }

        // Prefer common array sizes
        return count switch {
            <= DefaultCapacity => DefaultCapacity,
            <= 8 => 8,
            <= 16 => 16,
            <= 32 => 32,
            <= 64 => 64,
            <= 128 => 128,
            <= 256 => 256,
            _ => count
        };
    }

    private int LastIndex => _capacity - 1;

    /// <summary>
    /// Returns a new <see cref="TElement[]"/> containing all elements of the builder.
    /// The builder cannot be used after this operation.
    /// </summary>
    /// <returns>A new array containing all the elements of the builder</returns>
    public TElement[] ToArrayAndFree() {
        var array = new TElement[_count];

        SlicedElements.CopyTo(array);
        Free();

        return array;
    }

    /// <summary>
    /// Returns a new <see cref="ImmutableArray{T}"/> containing all elements of the builder.
    /// The builder cannot be used after this operation.
    /// </summary>
    /// <returns>A new array containing all the elements of the builder</returns>
    public ImmutableArray<TElement> ToImmutableArrayAndFree() {
        // The ImmutableArray constructor will create a new array from the span,
        // so we can give it a cut-down version of the array we rented, avoiding
        // allocating the correctly sized array twice.
        var array = ImmutableArray.Create(SlicedElements);
        Free();
        return array;
    }

    /// <summary>
    /// Returns a new <see cref="Span{T}"/> containing all elements of the builder.
    /// The builder cannot be used after this operation.
    /// </summary>
    /// <returns>A new span containing all the elements of the builder</returns>
    public Span<TElement> ToSpanAndFree() {
        Span<TElement> backingArray = new TElement[_count];

        SlicedElements.CopyTo(backingArray);
        Free();

        return backingArray;
    }

    /// <summary>
    /// Returns a new <see cref="ReadOnlySpan{T}"/> containing all elements of the builder.
    /// The builder cannot be used after this operation.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<TElement> ToReadOnlySpanAndFree() {
        return new ReadOnlySpan<TElement>(ToArrayAndFree());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TemporarySpan<TElement> AsRefForfeitOwnership() {
        return new TemporarySpan<TElement>(SlicedElements, _rentedArray);
    }

    /// <summary>
    /// Returns a span that includes all elements in the builder, useful for
    /// iterating over the elements or copying them to another span.
    /// Consumers DO NOT own the returned span and MUST NOT use it after
    /// this builder instance has been used again, mutate it,
    /// pass it to another method or otherwise leak it outside of it's owned context.
    /// The backing array of the span is still used by the builder and will be changed
    /// / updated it for future operations.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<TElement> DangerousAsSpanWithoutOwnership() {
        return SlicedElements;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<TElement> DangerousAsWriteableSpanWithoutOwnership() {
        return SlicedElements;
    }

    internal void Free() {
        FreeBackingArray();
        this = default;
    }

    public void AddRange(IEnumerable<TElement> elements) {
        foreach (var element in elements) {
            Add(element);
        }
    }

    public void AddRange(ReadOnlySpan<TElement> elements) {
        var newCount = _count + elements.Length;
        if (newCount <= LastIndex) {
            elements.CopyTo(_elements[_count..]);
            _count = newCount;
            return;
        }

        Grow(newCount);

        elements.CopyTo(_elements[_count..]);
        _count = newCount;
    }

    public void Add(TElement element) {
        if (_count < LastIndex) {
            AddElement(element);
            return;
        }

        if (_count >= LastIndex) {
            Grow(_count + 1);
            AddElement(element);
            return;
        }

        throw new InvalidOperationException("Oops!");
    }

    private void AddElement(TElement element) {
        _elements[_count] = element;
        _count++;
    }

    public void GrowTo(int capacity) {
        if (capacity < _count) {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        Grow(capacity);
    }

    private void Grow(int capacity) {
        Debug.Assert(_count < capacity);

        if (_rentedArray.Length >= capacity) {
            _capacity = capacity;
            return;
        }

        // Don't jump for the more common grow case
        if (_capacity is not 0) {
            GrowWithoutInitValue(capacity);
            return;
        }

        if (capacity < DefaultCapacity) {
            _capacity = DefaultCapacity;
            SetBackingArray(AllocateArray(_capacity));
        }

        GrowWithoutInitValue(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GrowWithoutInitValue(int capacity) {
        // TODO: When we grow, don't return the current buffer but instead
        // rent a new one and keep it as the continuation of the current one.
        var newCapacity = _capacity * 2;
        if (newCapacity < capacity) {
            newCapacity = capacity;
        }

        _capacity = newCapacity;

        var resizedElements = AllocateArray(_capacity);
        _elements.CopyTo(resizedElements);
        FreeBackingArray();
        SetBackingArray(resizedElements);
    }

    [MemberNotNull(nameof(_elements))]
    [MemberNotNull(nameof(_rentedArray))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBackingArray(TElement[] array) {
        _elements = _rentedArray = array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TElement[] AllocateArray(int size) {
        return ArrayPool<TElement>.Shared.Rent(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FreeBackingArray() {
        ArrayPool<TElement>.Shared.Return(_rentedArray);
    }
}

public static class ArrayBuilderExtensions {
    public static int IndexOf<T>(this ArrayBuilder<T> builder, T value) where T : IEquatable<T> {
        return builder.DangerousAsSpanWithoutOwnership().IndexOf(value);
    }

    public static bool Contains<T>(this ArrayBuilder<T> builder, T value) where T : IEquatable<T> {
        return builder.IndexOf(value) is not -1;
    }

    public static ref T? TryGet<T>(this ArrayBuilder<T> builder, T element) where T : IEquatable<T> {
        var index = builder.IndexOf(element);
        if (index is not -1) {
            return ref builder.DangerousAsWriteableSpanWithoutOwnership()[index]!;
        }

        return ref Unsafe.NullRef<T>()!;
    }

    public static unsafe string ToStringAndFree(this ArrayBuilder<char> builder) {
        var chars = builder.DangerousAsSpanWithoutOwnership();

        // The string ctor should copy all chars to its own buffer and
        // allow us to free the rented array, without breaking the string.
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.cs,151
        string str;
        fixed (char* p = chars) {
            str = new string(p, 0, chars.Length);
        }

        builder.Free();
        return str;
    }
}
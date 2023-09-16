using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mumei.DependencyInjection.Roslyn;

public ref struct ArrayBuilder<TElement> {
  private const int DefaultInitCapacity = 4;

  private Span<TElement> _elements = Array.Empty<TElement>();
  private TElement[] _rentedArray = Array.Empty<TElement>();
  private int _count = 0;
  private int _capacity = 0;

  public ArrayBuilder() { }

  private int LastIndex => _capacity - 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TElement[] ToArrayAndFree() {
    var array = new TElement[_count];

    AsUnsafeSpanWithoutOwnership().CopyTo(array);
    ArrayPool<TElement>.Shared.Return(_rentedArray);

    return array;
  }

  public ImmutableArray<TElement> ToImmutableArrayAndFree() {
    var elementsWithActualLength = AsUnsafeSpanWithoutOwnership();
    // The ImmutableArray constructor will create a new array from the span,
    // so we can give it a cut-down version of the array we rented, avoiding
    // allocating the correctly sized array twice.
    var array = ImmutableArray.Create(elementsWithActualLength);
    ArrayPool<TElement>.Shared.Return(_rentedArray);
    return array;
  }

  public Span<TElement> ToSpanAndFree() {
    Span<TElement> backingArray = new TElement[_count];

    AsUnsafeSpanWithoutOwnership().CopyTo(backingArray);
    ArrayPool<TElement>.Shared.Return(_rentedArray);

    return backingArray;
  }

  public ReadOnlySpan<TElement> ToReadOnlySpanAndFree() {
    return new ReadOnlySpan<TElement>(ToArrayAndFree());
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
  public ReadOnlySpan<TElement> AsUnsafeSpanWithoutOwnership() {
    return _elements[.._count];
  }

  public void AddRange(IEnumerable<TElement> elements) {
    foreach (var element in elements) {
      Add(element);
    }
  }

  public void AddRange(Span<TElement> elements) {
    var newCount = _count + elements.Length;
    if (newCount <= LastIndex) {
      elements.CopyTo(_elements[_count..]);
      _count = newCount;
      return;
    }

    do {
      Grow();
    } while (LastIndex < newCount);

    elements.CopyTo(_elements[_count..]);
    _count = newCount;
  }

  public void Add(TElement element) {
    if (_count < LastIndex) {
      AddElement(element);
      return;
    }

    if (_count == LastIndex) {
      Grow();
      AddElement(element);
      return;
    }

    throw new InvalidOperationException("Oops!");
  }

  private void AddElement(TElement element) {
    _count++;
    _elements[_count] = element;
  }

  private void Grow() {
    if (_capacity is 0) {
      _capacity = DefaultInitCapacity;
      _elements = _rentedArray = ArrayPool<TElement>.Shared.Rent(_capacity);
      return;
    }

    _capacity *= 2;

    var resizedElements = ArrayPool<TElement>.Shared.Rent(_capacity);
    _elements.CopyTo(resizedElements);
    ArrayPool<TElement>.Shared.Return(_rentedArray);
    _rentedArray = resizedElements;
    _elements = resizedElements;
  }
}
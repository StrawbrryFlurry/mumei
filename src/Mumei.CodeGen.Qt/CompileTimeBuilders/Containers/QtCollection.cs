using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Mumei.CodeGen.Qt.Containers;

// It might make sense to not use SmallElementArray here, since we can't efficiently
// combine it with IQtMemoryAccessor which breaks lots of use cases where we want to
// format the elements into common patterns.
public readonly struct QtCollection<TElement> : IReadOnlyCollection<TElement> {
    private readonly SmallElementArray<TElement> _elements = default;
    private readonly TElement[]? _largeElements;

    public int Count => _largeElements?.Length ?? _elements.Length;

    public QtCollection() { }

    [UnscopedRef]
    public Enumerator GetEnumerator() {
        return new Enumerator(in this);
    }

    IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<TElement>) this).GetEnumerator();
    }

    [UnscopedRef]
    public readonly ref readonly TElement this[int index] {
        get {
            if (_largeElements is not null) {
                return ref _largeElements[index];
            }

            return ref _elements[index];
        }
    }

    public ref struct Enumerator(ref readonly QtCollection<TElement> collection) {
        private int _index;
        private readonly QtCollection<TElement> _collection = collection;

        [UnscopedRef]
        public ref readonly TElement Current => ref _collection[_index];

        public bool MoveNext() {
            if (_index >= _collection.Count) return false;

            _index++;
            return _index < _collection.Count;
        }
    }
}
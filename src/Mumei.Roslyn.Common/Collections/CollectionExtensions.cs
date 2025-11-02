using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Mumei.Roslyn;

internal static class CollectionExtensions {
    extension<T>(IEnumerable<T> source) {
        public IEnumerator<T> GetEnumeratorInterfaceImplementation() {
            return source.GetEnumerator();
        }
    }

    extension<T>(ImmutableArray<T> source) {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmutableArray<T> EnsureInitialized() {
            return source.IsDefault ? ImmutableArray<T>.Empty : source;
        }
    }
}
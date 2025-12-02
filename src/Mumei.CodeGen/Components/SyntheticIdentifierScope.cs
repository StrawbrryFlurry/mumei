using System.Collections.Concurrent;

namespace Mumei.CodeGen.Components;

internal sealed class SyntheticIdentifierScope : ISyntheticIdentifierScope {
    private ConcurrentDictionary<string, TrackedIdentifier>? _identifiers;

    public string MakeUnique(string baseName) {
        var trackedIdentifier = GetOrAddTrackedIdentifier(baseName);
        var id = trackedIdentifier.NextId();
        return $"{baseName}__{id}";
    }

    private TrackedIdentifier GetOrAddTrackedIdentifier(string baseName) {
        if (_identifiers is null) {
            var trackingInfo = new ConcurrentDictionary<string, TrackedIdentifier>();
            Interlocked.CompareExchange(ref _identifiers, trackingInfo, null);
        }

        return _identifiers.GetOrAdd(baseName, _ => new TrackedIdentifier());
    }

    private sealed class TrackedIdentifier {
        private int _counter = -1;

        public int NextId() {
            return Interlocked.Increment(ref _counter);
        }
    }
}

public interface ISyntheticIdentifierScope {
    public string MakeUnique(string baseName);
}
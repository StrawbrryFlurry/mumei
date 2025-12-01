using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Mumei.CodeGen.Components;
using Mumei.Roslyn;
using Mumei.Roslyn.Common.Polyfill;

namespace Mumei.CodeGen.Roslyn.RoslynCodeProviders;

internal sealed class CompilationHostOutputIdentifierResolver : IIdentifierResolver {
    private readonly ConcurrentDictionary<IdentifierScope, ScopeIdTracker> _scopeIds = new();

    public IdentifierScope GlobalScope { get; } = new(0);

    public CompilationHostOutputIdentifierResolver() {
        _scopeIds[GlobalScope] = new ScopeIdTracker(null, GlobalScope);
    }

    public string MakeUnique(IdentifierScope scope, string name) {
        var trackedScope = GetOrAddTrackedScope(scope);
        var nameBuilder = new ArrayBuilder<char>(stackalloc char[Math.Max(ArrayBuilder.InitSize, name.Length)]);
        nameBuilder.AddRange(name);
        trackedScope.IncrementAndAppendUniquePortion(ref nameBuilder);
        return nameBuilder.ToStringAndFree();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ScopeIdTracker GetOrAddTrackedScope(IdentifierScope scope) {
        var tracker = _scopeIds.GetOrAdd(
            scope,
            identifierScope => new ScopeIdTracker(
                _scopeIds[GlobalScope], // Since all scopes always have a unique portion we might not even need to track parents here
                identifierScope
            )
        );

        return tracker;
    }

    private sealed class ScopeIdTracker(
        ScopeIdTracker? parentTracker,
        IdentifierScope scope
    ) {
        private int _currentId = -1;

        public int ScopeId => scope.ScopeId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementAndAppendUniquePortion(ref ArrayBuilder<char> nameBuilder) {
            parentTracker?.AppendUniquePortion(ref nameBuilder);

            var nextId = Interlocked.Increment(ref _currentId);
            WriteScopeId(nextId, ref nameBuilder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendUniquePortion(ref ArrayBuilder<char> nameBuilder) {
            parentTracker?.AppendUniquePortion(ref nameBuilder);
            WriteScopeId(scope.ScopeId, ref nameBuilder);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteScopeId(int scopeId, ref ArrayBuilder<char> nameBuilder) {
            // Do this all in a single buffer so we don't resize multiple times because of the prefix '_'
            Span<char> buffer = stackalloc char[char.MaxAsciiIntLength + 1];
            buffer[0] = '_';
            var charsWritten = char.WriteIntAsAsciiChars((uint) scopeId, buffer[1..]);
            nameBuilder.AddRange(buffer[..(charsWritten + 1)]);
        }
    }
}
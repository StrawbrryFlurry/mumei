using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen.Roslyn;

public sealed class CompilationSymbolCache(Compilation compilation) {
    private static readonly ConditionalWeakTable<Compilation, CompilationSymbolCache> _caches = new();
    private static readonly ConditionalWeakTable<Compilation, CompilationSymbolCache>.CreateValueCallback CreateCache = compilation => new CompilationSymbolCache(compilation);

    private readonly WeakReference<Compilation> _compilation = new(compilation);
    private readonly Dictionary<Type, INamedTypeSymbol> _typeSymbols = [];

    public static INamedTypeSymbol? TryGetTypeInCompilation(Compilation c, Type t) {
        var cache = _caches.GetValue(c, CreateCache);
        return cache.TryGetTypeInCompilationCore(t);
    }

    private INamedTypeSymbol? TryGetTypeInCompilationCore(Type t) {
        if (_compilation.TryGetTarget(out var compilation)) {
            return compilation.GetTypeByMetadataName(t.FullName ?? t.Name);
        }

        return null;
    }
}
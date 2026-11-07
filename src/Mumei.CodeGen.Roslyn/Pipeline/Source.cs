using Microsoft.CodeAnalysis;

namespace Mumei.CodeGen;

/// <summary>A stream of equatable values, e.g. one per annotated type or intercepted call.</summary>
public class Source<T> {
    private readonly Lazy<IncrementalValuesProvider<T>> _provider;

    internal Source(GeneratorContext gen, IncrementalValuesProvider<T> provider) : this(gen, () => provider) { }

    private protected Source(GeneratorContext gen, Func<IncrementalValuesProvider<T>> provider) {
        Gen = gen;
        _provider = new Lazy<IncrementalValuesProvider<T>>(provider, LazyThreadSafetyMode.None);
    }

    internal GeneratorContext Gen { get; }

    /// <summary>The underlying Roslyn provider, for combining with plain Roslyn APIs.</summary>
    public IncrementalValuesProvider<T> Roslyn => _provider.Value;

    public Source<T> Where(Func<T, bool> predicate) {
        return new Source<T>(Gen, Roslyn.Where(predicate));
    }

    /// <summary>Transforms each item. The result must be equatable (records, primitives, Mumei models, <see cref="EquatableArray{T}"/>).</summary>
    public Source<TResult> Select<TResult>(Func<T, TResult> selector) {
        Cacheability.Ensure(typeof(TResult), nameof(Select));
        return new Source<TResult>(Gen, Roslyn.Select((x, _) => selector(x)));
    }

    public Source<TResult> SelectMany<TResult>(Func<T, IEnumerable<TResult>> selector) {
        Cacheability.Ensure(typeof(TResult), nameof(SelectMany));
        return new Source<TResult>(Gen, Roslyn.SelectMany((x, _) => selector(x)));
    }

    /// <summary>Collects all items into a single value, e.g. to generate one file for all of them.</summary>
    public Value<EquatableArray<T>> Collect() {
        return new Value<EquatableArray<T>>(Gen, Roslyn.Collect().Select((x, _) => (EquatableArray<T>) x));
    }

    /// <summary>Pairs every item with a single value, e.g. options or information about the compilation.</summary>
    public Source<(T Item, TOther Value)> Combine<TOther>(Value<TOther> other) {
        return new Source<(T, TOther)>(Gen, Roslyn.Combine(other.Roslyn));
    }

    /// <summary>Generates code for each item. Everything added to <paramref name="generate"/>'s output is emitted as source.</summary>
    public void Generate(Action<T, SourceOutput> generate) {
        Gen.Roslyn.RegisterSourceOutput(Roslyn, (context, item) => SourceOutput.Run(context, output => generate(item, output)));
    }
}

/// <summary>A source whose items still have access to their symbol in <c>Where</c>/<c>Select</c>.</summary>
public sealed class SymbolSource<T, TSymbol> : Source<T> where TSymbol : ISymbol {
    private readonly SymbolOrigin<TSymbol> _origin;
    private readonly Func<SemanticContext<TSymbol>, Maybe<T>> _transform;

    internal SymbolSource(GeneratorContext gen, SymbolOrigin<TSymbol> origin, Func<SemanticContext<TSymbol>, Maybe<T>> transform)
        : base(gen, () => origin.Create(gen, transform)) {
        _origin = origin;
        _transform = transform;
    }

    /// <summary>Filters items using the full semantic model. The filter runs in the same pipeline step that creates the item.</summary>
    public SymbolSource<T, TSymbol> Where(Func<T, SemanticContext<TSymbol>, bool> predicate) {
        var transform = _transform;
        return new SymbolSource<T, TSymbol>(Gen, _origin, semantic => {
            var item = transform(semantic);
            return item.HasValue && predicate(item.Value, semantic) ? item : default;
        });
    }

    /// <summary>
    /// Transforms each item using the full semantic model. The result must be equatable,
    /// so extract what you need from the symbol (e.g. via <c>ToTypeRef()</c>) rather than returning it.
    /// </summary>
    public SymbolSource<TResult, TSymbol> Select<TResult>(Func<T, SemanticContext<TSymbol>, TResult> selector) {
        Cacheability.Ensure(typeof(TResult), nameof(Select));
        var transform = _transform;
        return new SymbolSource<TResult, TSymbol>(Gen, _origin, semantic => {
            var item = transform(semantic);
            return item.HasValue ? new Maybe<TResult>(true, selector(item.Value, semantic)) : default;
        });
    }
}

/// <summary>A single equatable value, e.g. options or all items of a <see cref="Source{T}"/>.</summary>
public sealed class Value<T> {
    internal Value(GeneratorContext gen, IncrementalValueProvider<T> provider) {
        Gen = gen;
        Roslyn = provider;
    }

    internal GeneratorContext Gen { get; }

    public IncrementalValueProvider<T> Roslyn { get; }

    public Value<TResult> Select<TResult>(Func<T, TResult> selector) {
        Cacheability.Ensure(typeof(TResult), nameof(Select));
        return new Value<TResult>(Gen, Roslyn.Select((x, _) => selector(x)));
    }

    public Value<(T Left, TOther Right)> Combine<TOther>(Value<TOther> other) {
        return new Value<(T, TOther)>(Gen, Roslyn.Combine(other.Roslyn));
    }

    public void Generate(Action<T, SourceOutput> generate) {
        Gen.Roslyn.RegisterSourceOutput(Roslyn, (context, item) => SourceOutput.Run(context, output => generate(item, output)));
    }
}

public static class SourceExtensions {
    /// <summary>Generates code for each item together with the value it was combined with.</summary>
    public static void Generate<T, TOther>(this Source<(T Item, TOther Value)> source, Action<T, TOther, SourceOutput> generate) {
        source.Generate((x, output) => generate(x.Item, x.Value, output));
    }

    /// <summary>Generates code for the two combined values.</summary>
    public static void Generate<T, TOther>(this Value<(T Left, TOther Right)> value, Action<T, TOther, SourceOutput> generate) {
        value.Generate((x, output) => generate(x.Left, x.Right, output));
    }

    /// <summary>Generates one interceptor per call.</summary>
    public static void Intercept(this Source<CallSite> calls, Action<InterceptorBuilder> intercept) {
        calls.Generate((call, output) => output.Intercept(call, intercept));
    }

    /// <summary>Generates one interceptor per call, with access to the value the calls were combined with.</summary>
    public static void Intercept<TOther>(this Source<(CallSite Item, TOther Value)> calls, Action<InterceptorBuilder, TOther> intercept) {
        calls.Generate((call, value, output) => output.Intercept(call, i => intercept(i, value)));
    }
}

internal readonly record struct Maybe<T>(bool HasValue, T Value);

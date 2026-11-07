using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Mumei.CodeGen;

/// <summary>The entry point for declaring what a <see cref="MumeiGenerator"/> generates.</summary>
public sealed class GeneratorContext {
    internal GeneratorContext(IncrementalGeneratorInitializationContext roslyn) {
        Roslyn = roslyn;
    }

    /// <summary>The underlying Roslyn context, for anything Mumei doesn't cover.</summary>
    public IncrementalGeneratorInitializationContext Roslyn { get; }

    /// <summary>All types annotated with the attribute, e.g. <c>gen.TypesWith("MyLib.ReprAttribute")</c>.</summary>
    public SymbolSource<TypeModel, INamedTypeSymbol> TypesWith(string attributeMetadataName) {
        return new SymbolSource<TypeModel, INamedTypeSymbol>(
            this,
            new AttributeOrigin<INamedTypeSymbol>(attributeMetadataName, node => node is BaseTypeDeclarationSyntax),
            semantic => new Maybe<TypeModel>(true, semantic.Symbol.ToTypeModel())
        );
    }

    /// <summary>
    /// All types declared in the compilation that match <paramref name="where"/>, e.g.
    /// <c>gen.Types(where: t => t.Implements("MyLib.IHandler"))</c>.
    /// Prefer <see cref="TypesWith"/> where possible: it is much cheaper.
    /// </summary>
    public SymbolSource<TypeModel, INamedTypeSymbol> Types(Func<TypeModel, bool>? where = null) {
        return new SymbolSource<TypeModel, INamedTypeSymbol>(
            this,
            new DeclarationOrigin<INamedTypeSymbol>(node => node is BaseTypeDeclarationSyntax),
            semantic => {
                var type = semantic.Symbol.ToTypeModel();
                return where is null || where(type) ? new Maybe<TypeModel>(true, type) : default;
            }
        );
    }

    /// <summary>All methods annotated with the attribute.</summary>
    public SymbolSource<MethodModel, IMethodSymbol> MethodsWith(string attributeMetadataName) {
        return new SymbolSource<MethodModel, IMethodSymbol>(
            this,
            new AttributeOrigin<IMethodSymbol>(attributeMetadataName, node => node is MethodDeclarationSyntax),
            semantic => new Maybe<MethodModel>(true, semantic.Symbol.ToMethodModel())
        );
    }

    /// <summary>All interceptable calls to methods named <paramref name="methodName"/>, regardless of the type they're declared on.</summary>
    public SymbolSource<CallSite, IMethodSymbol> Calls(string methodName) {
        return CallSites(methodName, static _ => true);
    }

    /// <summary>
    /// All interceptable calls to <paramref name="methodName"/> declared on the type, e.g.
    /// <c>gen.CallsTo("System.Collections.Generic.List`1", "Add")</c>. Extension methods are matched by their declaring static class.
    /// </summary>
    public SymbolSource<CallSite, IMethodSymbol> CallsTo(string typeMetadataName, string methodName) {
        return CallSites(methodName, method => method.ContainingType.OriginalDefinition.GetMetadataName() == typeMetadataName);
    }

    /// <summary>All interceptable calls to <paramref name="methodName"/> declared on the type, e.g. <c>gen.CallsTo(typeof(Console), "WriteLine")</c>.</summary>
    public SymbolSource<CallSite, IMethodSymbol> CallsTo(Type type, string methodName) {
        var typeDefinition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        return CallsTo(RuntimeTypeSerializer.GetMetadataName(typeDefinition), methodName);
    }

    /// <summary>All interceptable calls to methods annotated with the attribute.</summary>
    public SymbolSource<CallSite, IMethodSymbol> CallsToMethodsWith(string attributeMetadataName) {
        return CallSites(null, method => (method.ReducedFrom ?? method).OriginalDefinition.GetAttributes()
            .Any(a => a.AttributeClass?.GetMetadataName() == attributeMetadataName));
    }

    /// <summary>Reads MSBuild properties and global analyzer config, e.g. <c>gen.BuildOptions(o => o.Property("RootNamespace"))</c>.</summary>
    public Value<T> BuildOptions<T>(Func<BuildOptions, T> selector) {
        Cacheability.Ensure(typeof(T), nameof(BuildOptions));
        return new Value<T>(this, Roslyn.AnalyzerConfigOptionsProvider.Select((x, _) => selector(new BuildOptions(x.GlobalOptions))));
    }

    /// <summary>
    /// Extracts information from the compilation, e.g. <c>gen.Compilation(c => c.AssemblyName)</c>.
    /// This runs on every change, so keep it cheap; the result must be equatable.
    /// </summary>
    public Value<T> Compilation<T>(Func<Compilation, T> selector) {
        Cacheability.Ensure(typeof(T), nameof(Compilation));
        return new Value<T>(this, Roslyn.CompilationProvider.Select((x, _) => selector(x)));
    }

    /// <summary>Wraps a plain Roslyn pipeline so it can be generated from with Mumei.</summary>
    public Source<T> FromRoslyn<T>(Func<IncrementalGeneratorInitializationContext, IncrementalValuesProvider<T>> provider) {
        Cacheability.Ensure(typeof(T), nameof(FromRoslyn));
        return new Source<T>(this, provider(Roslyn));
    }

    /// <inheritdoc cref="FromRoslyn{T}(Func{IncrementalGeneratorInitializationContext,IncrementalValuesProvider{T}})"/>
    public Value<T> FromRoslyn<T>(Func<IncrementalGeneratorInitializationContext, IncrementalValueProvider<T>> provider) {
        Cacheability.Ensure(typeof(T), nameof(FromRoslyn));
        return new Value<T>(this, provider(Roslyn));
    }

    /// <summary>
    /// Adds a source file that doesn't depend on the compilation, e.g. a marker attribute.
    /// It is available to the user's code and to the semantic model the other sources see.
    /// </summary>
    public void AddSource(string hintName, string text) {
        Roslyn.RegisterPostInitializationOutput(context => context.AddSource(hintName, SourceText.From(text, Encoding.UTF8)));
    }

    /// <inheritdoc cref="AddSource(string,string)"/>
    public void AddSource(string hintName, Action<SourceFileBuilder> build) {
        var file = new SourceFileBuilder();
        build(file);
        AddSource(hintName, file.ToString());
    }

    private SymbolSource<CallSite, IMethodSymbol> CallSites(string? methodName, Func<IMethodSymbol, bool> methodPredicate) {
        return new SymbolSource<CallSite, IMethodSymbol>(
            this,
            new CallOrigin(methodName),
            semantic => methodPredicate(semantic.Symbol)
                        && semantic.Node is InvocationExpressionSyntax invocation
                        && invocation.ToCallSite(semantic.Symbol, semantic.SemanticModel, semantic.CancellationToken) is { } call
                ? new Maybe<CallSite>(true, call)
                : default
        );
    }
}

/// <summary>MSBuild properties and global analyzer config options.</summary>
public readonly struct BuildOptions {
    private readonly AnalyzerConfigOptions _options;

    internal BuildOptions(AnalyzerConfigOptions options) {
        _options = options;
    }

    /// <summary>Reads a raw option, e.g. <c>build_property.RootNamespace</c>.</summary>
    public string? Get(string key) {
        return _options.TryGetValue(key, out var value) && value.Length > 0 ? value : null;
    }

    /// <summary>
    /// Reads an MSBuild property. The property must be made visible to the compiler with
    /// <c>&lt;CompilerVisibleProperty Include="Name" /&gt;</c>.
    /// </summary>
    public string? Property(string name) {
        return Get($"build_property.{name}");
    }

    /// <summary>Whether an MSBuild property is set to <c>true</c>.</summary>
    public bool IsEnabled(string property) {
        return string.Equals(Property(property), "true", StringComparison.OrdinalIgnoreCase);
    }
}

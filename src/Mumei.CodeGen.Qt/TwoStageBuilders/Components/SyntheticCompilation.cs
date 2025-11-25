using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticCompilation {
    // ReSharper disable once InconsistentNaming
    public IλInternalCompilerApi λCompilerApi { get; }
    public Compilation UnderlyingCompilation { get; }

    public ISyntheticNamespace NamespaceFromCompilation(string name);

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(string name);

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(
        string name,
        Action<TClassDefinition> inputBinder
    ) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();

    public string MakeUniqueName(string name);

    public T? Synthesize<T>(object? constructable, T? defaultValue = default);
    public T? SynthesizeOptional<T>(object? constructable);

    public ITypeSymbol TypeFromCompilation<T>();

    public ISyntheticType GetType(ITypeSymbol typeSymbol);

    public ISyntheticType GetType(Type type);

    public ISyntheticType GetType(TypeSyntax typeSyntax);
}

/// <summary>
/// API Surface required by the compiler implementation to declare synthetic components.
/// </summary>
// ReSharper disable once InconsistentNaming
public interface IλInternalCompilerApi {
    /// <summary>
    /// Generates a unique name inside this compilation that is unrelated
    /// to the compilations tracked components.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string MakeArbitraryUniqueName(string name);

    public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name);

    public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder)
        where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();

    public string NextId();
}

/// <summary>
/// Stores synthetic code components that can be built into compilation units or Synthesized trees.
/// Components that reference other components use this to resolve those dependencies.
/// </summary>
internal sealed class SyntheticCompilation(Compilation compilation) : ISyntheticCompilation {
    // ReSharper disable once InconsistentNaming
    public IλInternalCompilerApi λCompilerApi => field ??= new QtSyntheticCompilationCompilerApi(this);
    public Compilation UnderlyingCompilation => compilation;

    public ISyntheticNamespace NamespaceFromCompilation(string name) {
        return new QtSyntheticNamespace(name);
    }

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(string name) {
        return new QtSyntheticClassBuilder<CompileTimeUnknown>(this);
    }

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(
        string name,
        Action<TClassDefinition> inputBinder
    ) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
        throw new NotSupportedException();
    }

    public string MakeUniqueName(string name) {
        return null!;
    }

    public T? Synthesize<T>(object? constructable, T? defaultValue = default) {
        return FragmentConstructor.ConstructFragment(this, constructable, defaultValue);
    }

    public T? SynthesizeOptional<T>(object? constructable) {
        return FragmentConstructor.ConstructOptionalFragment<T>(this, constructable);
    }

    public ITypeSymbol TypeFromCompilation<T>() {
        var type = compilation.GetTypeByMetadataName(typeof(T).FullName!);
        return type ?? throw new InvalidOperationException("Type not found in compilation: " + typeof(T).FullName);
    }

    public ISyntheticType GetType(ITypeSymbol typeSymbol) {
        return new RoslynSyntheticType(typeSymbol);
    }

    public ISyntheticType GetType(Type type) {
        return new RuntimeSyntheticType(type);
    }

    public ISyntheticType GetType(TypeSyntax typeSyntax) {
        var type = compilation.GetSemanticModel(typeSyntax.SyntaxTree)?.GetSymbolInfo(typeSyntax);
        if (type is null || type.Value.Symbol is not ITypeSymbol typeSymbol) {
            throw new InvalidOperationException($"Type not found for syntax: {typeSyntax}");
        }

        return new RoslynSyntheticType(typeSymbol);
    }

    private sealed class QtSyntheticCompilationCompilerApi(SyntheticCompilation compilation) : IλInternalCompilerApi {
        private int _internalTrackingId = 0;

        public string MakeArbitraryUniqueName(string name) {
            return $"{name}__{_internalTrackingId++}";
        }

        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name) {
            return new QtSyntheticClassBuilder<TClassDefinition>(compilation).WithName(name);
        }

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
            return classBuilder;
        }

        private UniqueNameGeneratorComponent? _uniqueNameGenerator;

        public string NextId() {
            return (_uniqueNameGenerator ??= new UniqueNameGeneratorComponent()).MakeUnique("");
        }
    }
}
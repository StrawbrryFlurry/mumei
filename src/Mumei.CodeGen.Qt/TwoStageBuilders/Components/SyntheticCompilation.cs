using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.CodeGen.Qt.Qt;
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

/// <summary>
/// Stores synthetic code components that can be built into compilation units or Synthesized trees.
/// Components that reference other components use this to resolve those dependencies.
/// </summary>
public sealed class SyntheticCompilation(Compilation compilation) {
    // ReSharper disable once InconsistentNaming
    public IλInternalCompilerApi λCompilerApi => field ??= new QtSyntheticCompilationCompilerApi(this);

    public ISyntheticNamespace NamespaceFromCompilation(string name) {
        return new QtSyntheticNamespace(name);
    }

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(string name) {
        return new SyntheticClassBuilder<CompileTimeUnknown>(this);
    }

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(
        string name,
        Action<TClassDefinition> inputBinder
    ) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
        throw new NotSupportedException();
    }

    public ISyntheticMethod Method<TMethodDefinition>(
        Action<TMethodDefinition> inputBinder,
        Func<TMethodDefinition, Delegate> methodSelector
    ) where TMethodDefinition : SyntheticMethodDefinition, new() {
        throw new NotSupportedException();
    }

    public ISyntheticMethod InterceptorMethod<TMethodDefinition>(
        InvocationExpressionSyntax invocationToIntercept,
        Action<TMethodDefinition> inputBinder,
        Func<TMethodDefinition, Delegate> methodSelector
    ) where TMethodDefinition : SyntheticMethodDefinition, new() {
        throw new NotSupportedException();
    }

    public string MakeUniqueName(string name) {
        return null!;
    }

    public SynthesizedClassDeclaration Synthesize<TClass>(ISyntheticClassBuilder<TClass> builder) {
        return new SynthesizedClassDeclaration();
    }

    public SynthesizedNamespace Synthesize(ISyntheticNamespace ns) {
        return new SynthesizedNamespace();
    }

    public ITypeSymbol TypeFromCompilation<T>() {
        var type = compilation.GetTypeByMetadataName(typeof(T).FullName!);
        return type ?? throw new InvalidOperationException("Type not found in compilation: " + typeof(T).FullName);
    }

    /// <summary>
    /// API Surface required by the compiler implementation to declare synthetic components.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public interface IλInternalCompilerApi {
        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name);

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder)
            where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();
    }

    private sealed class QtSyntheticCompilationCompilerApi(SyntheticCompilation compilation) : IλInternalCompilerApi {
        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name) {
            return new SyntheticClassBuilder<TClassDefinition>(compilation).WithName(name);
        }

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
            throw new NotImplementedException();
        }
    }
}
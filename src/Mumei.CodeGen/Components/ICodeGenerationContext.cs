using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen.Components;

public interface ICodeGenerationContext {
    public IΦInternalCompilerApi ΦCompilerApi { get; }

    public ISyntheticCodeBlock Block(RenderFragment renderBlock);
    public ISyntheticCodeBlock Block<TInput>(TInput input, Action<IRenderTreeBuilder, TInput> renderBlock);

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(string name);
    public ISyntheticNamespace Namespace(params ReadOnlySpan<string> namespaceSegments);

    public void Emit(string hintName, ISyntheticNamespace toEmit);
    public void EmitIncremental(string hintName, ISyntheticNamespace toEmit);

    public void RegisterContextProvider<TProvider>(TProvider provider) where TProvider : ICodeGenerationContextProvider;
    public TProvider GetContextProvider<TProvider>() where TProvider : ICodeGenerationContextProvider;

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(
        string name,
        Action<TClassDefinition> inputBinder
    ) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();

    public interface IΦInternalCompilerApi {

        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name);

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder)
            where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();

        public ImmutableArray<(string TrackingName, ImmutableArray<ISyntheticNamespace> Namespaces)> EnumerateNamespacesToEmit();
    }
}
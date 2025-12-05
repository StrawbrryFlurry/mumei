using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

public interface ICodeGenerationContext : IEquatable<ICodeGenerationContext> {
    public IΦInternalCompilerApi ΦCompilerApi { get; }

    public ISyntheticNamespace GlobalNamespace { get; }

    public ISyntheticCodeBlock Block(RenderFragment renderBlock);
    public ISyntheticCodeBlock Block<TInput>(TInput input, Action<IRenderTreeBuilder, TInput> renderBlock);

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(SyntheticIdentifier name);
    public ISyntheticNamespaceBuilder Namespace(params ReadOnlySpan<string> namespaceSegments);

    public ISyntheticType Type(Type type);

    public ISyntheticParameter Parameter(
        ISyntheticType type,
        SyntheticIdentifier name,
        ParameterAttributes attributes = ParameterAttributes.None
    );

    public ISyntheticTypeParameter TypeParameter(SyntheticIdentifier name);

    public ISyntheticParameterList ParameterList(ReadOnlySpan<ISyntheticParameter> parameters);

    public void Emit(string hintName, ISyntheticDeclaration toEmit);
    public void EmitIncremental(string hintName, ISyntheticDeclaration toEmit);
    public void EmitUnique(string hintName, ISyntheticDeclaration toEmit);

    public void RegisterContextProvider<TProvider>(TProvider provider) where TProvider : ICodeGenerationContextProvider;
    public TProvider GetContextProvider<TProvider>() where TProvider : ICodeGenerationContextProvider;

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(
        SyntheticIdentifier name,
        Action<TClassDefinition> inputBinder
    ) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();

    public interface IΦInternalCompilerApi {
        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(
            SyntheticIdentifier name,
            ISyntheticDeclaration? parentDeclaration
        );

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder)
            where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();

        public ImmutableArray<(SyntheticIdentifier TrackingName, ImmutableArray<ISyntheticDeclaration> Declarations)> EnumerateDeclarationsToEmit();
    }
}
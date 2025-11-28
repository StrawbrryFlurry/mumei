using System.Collections.Immutable;
using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen.Components;

public interface ICodeGenerationContext : IComponentSynthesizer {
    public ISyntheticCodeBlock Block(RenderFragment renderBlock);
    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(string name);
    public ISyntheticNamespace Namespace(params ReadOnlySpan<string> namespaceSegments);

    public void Emit(string hintName, ISyntheticNamespace toEmit);

    public void RegisterSynthesisProvider<TProvider>(TProvider provider) where TProvider : ISynthesisProvider;
    public TProvider GetSynthesisProvider<TProvider>() where TProvider : ISynthesisProvider;

    public IΦInternalCompilerApi ΦCompilerApi { get; }

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(
        string name,
        Action<TClassDefinition> inputBinder
    ) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();

    public void TrackForEmission(string hintName, ISyntheticNamespace ns);

    public interface IΦInternalCompilerApi {
        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name);

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder)
            where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new();

        public ImmutableArray<(string TrackingName, ImmutableArray<ISyntheticNamespace> Namespaces)> EnumerateNamespacesToEmit();
    }
}
namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext {
    public ICodeGenerationContext.IΦInternalCompilerApi ΦCompilerApi => field ??= new CompilerApiImpl(this);

    private sealed class CompilerApiImpl(CSharpCodeGenerationContext context) : ICodeGenerationContext.IΦInternalCompilerApi {
        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name) {
            return new QtSyntheticClassBuilder<TClassDefinition>(new ConstantSyntheticIdentifier(name), context);
        }

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
            return classBuilder;
        }

        public ImmutableArray<(ISyntheticIdentifier TrackingName, ImmutableArray<ISyntheticNamespace> Namespaces)> EnumerateNamespacesToEmit() {
            return context._namespacesToEmit.Select(x => (x.Key, x.Value.Values.ToImmutableArray())).ToImmutableArray();
        }
    }
}
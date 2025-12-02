namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext {
    public ICodeGenerationContext.IΦInternalCompilerApi ΦCompilerApi => field ??= new CompilerApiImpl(this);

    private sealed class CompilerApiImpl(
        CSharpCodeGenerationContext context
    ) : ICodeGenerationContext.IΦInternalCompilerApi {
        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(string name) {
            return new QtSyntheticClassBuilder<TClassDefinition>(name, context);
        }

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(ISyntheticClassBuilder<TClassDefinition> classBuilder) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
            return classBuilder;
        }

        public ImmutableArray<(SyntheticIdentifier TrackingName, ImmutableArray<ISyntheticDeclaration> Declarations)> EnumerateDeclarationsToEmit() {
            return context._emitGraph.EnumerateDeclarationsToEmit();
        }
    }
}
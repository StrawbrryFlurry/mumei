namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext {
    public ICodeGenerationContext.IΦInternalCompilerApi ΦCompilerApi { get; }

    private sealed class CompilerApiImpl(
        CSharpCodeGenerationContext context
    ) : ICodeGenerationContext.IΦInternalCompilerApi {
        public ISyntheticClassBuilder<TClassDefinition> DeclareClassBuilder<TClassDefinition>(
            SyntheticIdentifier name,
            ISyntheticDeclaration? parent
        ) {
            return new QtSyntheticClassBuilder<TClassDefinition>(name, parent ?? context._globalNamespaceBuilder, context);
        }

        public ISyntheticClassBuilder<TClassDefinition> TrackClass<TClassDefinition>(
            ISyntheticClassBuilder<TClassDefinition> classBuilder
        ) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
            return classBuilder;
        }

        public ImmutableArray<(SyntheticIdentifier TrackingName, ImmutableArray<ISyntheticDeclaration> Declarations)> EnumerateDeclarationsToEmit() {
            return context._emitGraph.EnumerateDeclarationsToEmit();
        }
    }
}
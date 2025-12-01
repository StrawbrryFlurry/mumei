using Mumei.CodeGen.Rendering;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Components;

internal sealed partial class CSharpCodeGenerationContext {
    public ISyntheticCodeBlock Block(RenderFragment renderBlock) {
        return new QtSyntheticRenderCodeBlock(renderBlock);
    }

    public ISyntheticCodeBlock Block<TInput>(TInput input, Action<IRenderTreeBuilder, TInput> renderBlock) {
        return new QtSyntheticRenderCodeBlock<TInput>(new RenderFragment<TInput>(input, renderBlock));
    }

    public ISyntheticClassBuilder<CompileTimeUnknown> DeclareClass(string name) {
        return new QtSyntheticClassBuilder<CompileTimeUnknown>(
            name,
            this
        );
    }

    public ISyntheticNamespace Namespace(params ReadOnlySpan<string> namespaceSegments) {
        var s = new ArrayBuilder<char>(stackalloc char[ArrayBuilder.InitSize]);
        for (var i = 0; i < namespaceSegments.Length; i++) {
            if (i > 0) {
                s.Add('.');
            }

            s.AddRange(namespaceSegments[i]);
        }

        return new QtSyntheticNamespace(s.ToStringAndFree());
    }

    public ISyntheticClassBuilder<TClassDefinition> DeclareClass<TClassDefinition>(string name, Action<TClassDefinition> inputBinder) where TClassDefinition : SyntheticClassDefinition<TClassDefinition>, new() {
        throw new NotImplementedException();
    }
}
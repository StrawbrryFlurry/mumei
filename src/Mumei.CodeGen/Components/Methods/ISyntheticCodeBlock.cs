using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components.Methods;

public interface ISyntheticCodeBlock { }

internal sealed class QtSyntheticRenderCodeBlock(RenderFragment renderFragment) : ISyntheticCodeBlock, ISyntheticConstructable<CodeBlockFragment> {
    public CodeBlockFragment Construct(ISyntheticCompilation compilation) {
        return CodeBlockFragment.Create(renderFragment);
    }
}

internal sealed class QtSyntheticRenderCodeBlock<TState>(RenderFragment<TState> renderFragment) : ISyntheticCodeBlock, ISyntheticConstructable<CodeBlockFragment> {
    public CodeBlockFragment Construct(ISyntheticCompilation compilation) {
        return CodeBlockFragment.Create(renderFragment, (renderTree, state) => state.Render(renderTree));
    }
}
using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

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
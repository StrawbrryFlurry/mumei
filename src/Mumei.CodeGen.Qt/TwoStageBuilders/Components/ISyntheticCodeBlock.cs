using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticCodeBlock { }

internal sealed class SyntheticRenderCodeBlock(RenderFragment renderFragment) : ISyntheticCodeBlock, ISyntheticConstructable<CodeBlockFragment> {
    public CodeBlockFragment Construct() {
        return CodeBlockFragment.Create(renderFragment);
    }
}

internal sealed class SyntheticRenderCodeBlock<TState>(RenderFragment<TState> renderFragment) : ISyntheticCodeBlock, ISyntheticConstructable<CodeBlockFragment> {
    public CodeBlockFragment Construct() {
        return CodeBlockFragment.Create(renderFragment, (renderTree, state) => state.Render(renderTree));
    }
}
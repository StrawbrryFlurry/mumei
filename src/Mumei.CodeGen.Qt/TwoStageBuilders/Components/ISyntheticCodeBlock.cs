using Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public interface ISyntheticCodeBlock { }

internal sealed class SyntheticRenderCodeBlock(RenderNode renderNode) : ISyntheticCodeBlock, ISyntheticConstructable<SynthesizedCodeBlock> {
    public SynthesizedCodeBlock Construct() {
        return SynthesizedCodeBlock.Create(renderNode);
    }
}
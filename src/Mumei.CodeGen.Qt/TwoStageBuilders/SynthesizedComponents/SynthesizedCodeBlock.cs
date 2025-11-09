namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct SynthesizedCodeBlock(string content) : IRenderNode {
    public static SynthesizedCodeBlock Create(string content) {
        return new SynthesizedCodeBlock(content);
    }

    public static SynthesizedCodeBlock Create<TState>(TState state, Action<TState, IRenderTreeBuilder> dynamicContent) {
        var builder = new SourceFileRenderTreeBuilder();
        dynamicContent(state, builder);
        return new SynthesizedCodeBlock(builder.GetSourceText());
    }

    public static SynthesizedCodeBlock Create(RenderNode dynamicContent) {
        return Create(dynamicContent, static (action, builder) => action(builder));
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Block(content);
    }
}
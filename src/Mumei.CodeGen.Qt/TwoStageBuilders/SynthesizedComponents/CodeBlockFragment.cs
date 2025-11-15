namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct CodeBlockFragment(string content) : IRenderFragment {
    public static CodeBlockFragment Create(string content) {
        return new CodeBlockFragment(content);
    }

    public static CodeBlockFragment Create<TState>(TState state, Action<IRenderTreeBuilder, TState> dynamicContent) {
        var builder = new SourceFileRenderTreeBuilder();
        dynamicContent(builder, state);
        return new CodeBlockFragment(builder.GetSourceText());
    }

    public static CodeBlockFragment Create(RenderFragment dynamicContent) {
        return Create(dynamicContent, static (builder, action) => action(builder));
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Block(content);
    }
}
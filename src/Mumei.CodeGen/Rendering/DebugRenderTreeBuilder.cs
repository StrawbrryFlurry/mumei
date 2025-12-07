namespace Mumei.CodeGen.Rendering;

internal sealed class DebugRenderTreeBuilder : GenericRenderTreeBuilder<string> {
    private SourceFileRenderTreeBuilder _innerBuilder = new();

    public static string Render<TFragment>(TFragment node) where TFragment : IRenderFragment {
        var builder = new DebugRenderTreeBuilder();
        return builder.RenderRootNode(node);
    }

    protected override void TextCore(ReadOnlySpan<char> s) {
        _innerBuilder.Text(s);
    }

    protected override void NewLineCore() {
        _innerBuilder.NewLine();
    }

    protected override void ValueCore<T>(in T value) {
        _innerBuilder.Value(value);
    }

    protected override void BlockCore(string s) {
        _innerBuilder.Block(s);
    }

    protected override void StartBlockCore() {
        _innerBuilder.StartBlock();
    }

    protected override void EndBlockCore() {
        _innerBuilder.EndBlock();
    }

    protected override void NodeCore<TRenderNode>(TRenderNode renderable) {
        _innerBuilder.Node(renderable);
    }

    public override string RenderRootNode<TRootNode>(TRootNode node) {
        return _innerBuilder.RenderRootNode(node);
    }
}
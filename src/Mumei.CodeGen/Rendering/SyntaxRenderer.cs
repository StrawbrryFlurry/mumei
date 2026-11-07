using System.Diagnostics;

namespace Mumei.CodeGen.Rendering;

[DebuggerTypeProxy(typeof(RenderTreeBuilderDebugView))]
public sealed class SourceFileRenderTreeBuilder : GenericRenderTreeBuilder<string>, IDisposable {
    private SyntaxWriter _sourceFile = new();

    protected override void TextCore(ReadOnlySpan<char> s) {
        _sourceFile.Write(s);
    }

    protected override void NewLineCore() {
        _sourceFile.WriteLine();
    }

    protected override void ValueCore<T>(in T value) {
        _sourceFile.Write(CSharpLiteral.Format(value));
    }

    protected override void BlockCore(string s) {
        _sourceFile.WriteBlock(s);
    }

    protected override void StartBlockCore() {
        _sourceFile.Indent();
    }

    protected override void EndBlockCore() {
        _sourceFile.Dedent();
    }

    protected override void NodeCore<TNode>(TNode renderable) {
        renderable.Render(this);
    }

    public override string RenderRootNode<TRootNode>(TRootNode node) {
        Node(node);
        return GetSourceText();
    }

    public override string ToString() {
        return GetSourceText();
    }

    internal string GetSourceText() {
        return _sourceFile.ToString();
    }

    public void Dispose() {
        _sourceFile.Clear();
    }

    private sealed class RenderTreeBuilderDebugView(SourceFileRenderTreeBuilder builder) {
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public string CurrentTree => builder.GetSourceText();

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        private IEnumerable<IRenderFragment> Graph => builder.DebugRenderGraph.Stack;
    }
}
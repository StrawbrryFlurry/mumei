namespace Mumei.CodeGen.Qt;

internal sealed class DebugRenderGraph {
    private List<IRenderFragment> _nodeStack = new();

    public void StartNode(IRenderFragment fragment) {
        _nodeStack.Add(fragment);
    }

    public void EndNode() {
        _nodeStack.RemoveAt(_nodeStack.Count - 1);
    }

    public string DebugView() {
        return string.Join(" -> ", _nodeStack.Select(n => n is IDebugRenderNodeFormattable debugNode ? debugNode.DescribeDebugNode() : n.ToString()));
    }

    public IEnumerable<IRenderFragment> Stack => _nodeStack;
}
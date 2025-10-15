namespace Mumei.CodeGen.Qt;

internal sealed class DebugRenderGraph {
    private List<IRenderNode> _nodeStack = new();

    public void StartNode(IRenderNode node) {
        _nodeStack.Add(node);
    }

    public void EndNode() {
        _nodeStack.RemoveAt(_nodeStack.Count - 1);
    }

    public string DebugView() {
        return string.Join(" -> ", _nodeStack.Select(n => n is IDebugRenderNodeFormattable debugNode ? debugNode.DescribeDebugNode() : n.ToString()));
    }
}
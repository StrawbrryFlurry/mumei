namespace Mumei.CodeGen.Rendering;

internal sealed class DebugRenderGraph {
    private List<IRenderFragment> _nodeStack = new();

    public void StartNode(IRenderFragment fragment) {
        _nodeStack.Add(fragment);
    }

    public void EndNode() {
        _nodeStack.RemoveAt(_nodeStack.Count - 1);
    }

    public string DebugView() {
        return string.Join(" -> ", _nodeStack.Select(n => {
            var debugNode = n as IDebugRenderFragmentFormattable;
            return debugNode != null ? debugNode.DescribeDebugNode() : n.ToString();
        }));
    }

    public IEnumerable<IRenderFragment> Stack => _nodeStack;
}
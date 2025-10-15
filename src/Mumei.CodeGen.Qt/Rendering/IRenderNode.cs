namespace Mumei.CodeGen.Qt;

public interface IRenderNode {
    public void Render(IRenderTreeBuilder renderTree);
}

public interface IDebugRenderNodeFormattable {
    public string DescribeDebugNode();
}
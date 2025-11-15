namespace Mumei.CodeGen.Qt;

public interface IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree);
}

public interface IDebugRenderNodeFormattable {
    public string DescribeDebugNode();
}
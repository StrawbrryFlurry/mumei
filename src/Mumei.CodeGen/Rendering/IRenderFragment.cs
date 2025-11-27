namespace Mumei.CodeGen.Rendering;

public interface IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree);
}

public interface IDebugRenderFragmentFormattable {
    public string DescribeDebugNode();
}
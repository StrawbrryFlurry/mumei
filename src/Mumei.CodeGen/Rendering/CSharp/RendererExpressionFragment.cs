namespace Mumei.CodeGen.Rendering.CSharp;

internal sealed class RendererExpressionFragment(RenderFragment render) : IRenderFragment {
    public static RendererExpressionFragment For(RenderFragment render) {
        return new RendererExpressionFragment(render);
    }

    public void Render(IRenderTreeBuilder renderTree) {
        render(renderTree);
    }
}
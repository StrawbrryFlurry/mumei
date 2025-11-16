namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

public static partial class DefaultRenderExpressionExtensions {
    public static void RenderTo(this Type type, IRenderTreeBuilder renderTree) {
        renderTree.QualifiedTypeName(type);
    }

    public static void RenderTo(this string str, IRenderTreeBuilder renderTree) {
        renderTree.Text(str);
    }

    public static void RenderTo(this int integer, IRenderTreeBuilder renderTree) {
        renderTree.Text(integer.ToString());
    }
}
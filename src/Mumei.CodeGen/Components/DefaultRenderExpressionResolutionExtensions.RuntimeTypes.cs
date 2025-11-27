using Mumei.CodeGen.Rendering;

namespace Mumei.CodeGen.Components;

public static partial class DefaultRenderExpressionExtensions {
    public static void RenderExpression(this Type type, IRenderTreeBuilder renderTree) {
        renderTree.QualifiedTypeName(type);
    }

    public static void RenderExpression(this string str, IRenderTreeBuilder renderTree) {
        renderTree.Text(str);
    }

    public static void RenderExpression(this int integer, IRenderTreeBuilder renderTree) {
        renderTree.Text(integer.ToString());
    }
}
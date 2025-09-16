using System.Runtime.CompilerServices;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt;

internal static class SyntaxRendererExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NewLine<TRenderer>(this TRenderer renderer) where TRenderer : IRenderer {
        renderer.Text("\n");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Line<TRenderer>(this TRenderer renderer, string line) where TRenderer : IRenderer {
        renderer.Text(line);
        renderer.NewLine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartCodeBlock<TRenderer>(this TRenderer renderer) where TRenderer : IRenderer {
        renderer.Line("{");
        renderer.StartBlock();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EndCodeBlock<TRenderer>(this TRenderer renderer) where TRenderer : IRenderer {
        renderer.EndBlock();
        renderer.Line("}");
    }

    public static void SeparatedList<TRenderer, TItem>(this TRenderer renderer, ReadOnlySpan<TItem> items) where TRenderer : IRenderer where TItem : IRenderNode {
        for (var i = 0; i < items.Length; i++) {
            if (i > 0) {
                renderer.Text(", ");
            }

            renderer.Node(items[i]);
        }
    }

    public static void SeparatedList<TRenderer, TItem, TRenderItem>(this TRenderer renderer, ReadOnlySpan<TItem> items, Func<TItem, TRenderItem> renderItemSelector) where TRenderer : IRenderer where TRenderItem : IRenderNode {
        for (var i = 0; i < items.Length; i++) {
            if (i > 0) {
                renderer.Text(", ");
            }

            var item = renderItemSelector(items[i]);
            renderer.Node(item);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void QualifiedTypeName<TRenderer>(this TRenderer renderer, Type type) where TRenderer : IRenderer {
        RuntimeTypeSerializer.RenderInto(renderer, type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TypeOf<TRenderer>(this TRenderer renderer, Type type) where TRenderer : IRenderer {
        renderer.Text("typeof(");
        RuntimeTypeSerializer.RenderInto(renderer, type);
        renderer.Text(")");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeExpressionRenderNode Expression(this IQtType type) {
        return new TypeExpressionRenderNode(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeOfRenderNode TypeOf(this IQtType type) {
        return new TypeOfRenderNode(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeFullNameRenderNode FullName(this IQtType type) {
        return new TypeFullNameRenderNode(type);
    }

    public readonly struct TypeExpressionRenderNode(IQtType instance) : IRenderNode {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Render(IRenderer renderer) {
            instance.RenderExpression(renderer);
        }
    }

    public readonly struct TypeOfRenderNode(IQtType instance) : IRenderNode {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Render(IRenderer renderer) {
            renderer.Text("typeof(");
            instance.RenderFullName(renderer);
            renderer.Text(")");
        }
    }

    public readonly struct TypeFullNameRenderNode(IQtType instance) : IRenderNode {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Render(IRenderer renderer) {
            instance.RenderFullName(renderer);
        }
    }
}
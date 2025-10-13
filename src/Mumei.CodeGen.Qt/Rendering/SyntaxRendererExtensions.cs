using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt;

internal static class SyntaxRendererExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NewLine(this IRenderTreeBuilder renderTree) {
        renderTree.Text("\n");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Line(this IRenderTreeBuilder renderTree, string line) {
        renderTree.Text(line);
        renderTree.NewLine();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartCodeBlock(this IRenderTreeBuilder renderTree) {
        renderTree.Line("{");
        renderTree.StartBlock();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EndCodeBlock(this IRenderTreeBuilder renderTree) {
        renderTree.EndBlock();
        renderTree.Line("}");
    }

    public static void SeparatedList<TItem>(this IRenderTreeBuilder renderTree, ReadOnlySpan<TItem> items) where TItem : IRenderNode {
        for (var i = 0; i < items.Length; i++) {
            if (i > 0) {
                renderTree.Text(", ");
            }

            renderTree.Node(items[i]);
        }
    }

    public static void SeparatedList<TItem, TRenderItem>(this IRenderTreeBuilder renderTree, ReadOnlySpan<TItem> items, Func<TItem, TRenderItem> renderItemSelector) where TRenderItem : IRenderNode {
        for (var i = 0; i < items.Length; i++) {
            if (i > 0) {
                renderTree.Text(", ");
            }

            var item = renderItemSelector(items[i]);
            renderTree.Node(item);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void QualifiedTypeName(this IRenderTreeBuilder renderTree, Type type) {
        RuntimeTypeSerializer.RenderInto(renderTree, type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TypeOf(this IRenderTreeBuilder renderTree, Type type) {
        renderTree.Text("typeof(");
        RuntimeTypeSerializer.RenderInto(renderTree, type);
        renderTree.Text(")");
    }

    public readonly struct TypeExpressionRenderNode(IQtType instance) : IRenderNode {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Render(IRenderTreeBuilder renderTree) {
            instance.RenderExpression(renderTree);
        }
    }

    public readonly struct TypeOfRenderNode(IQtType instance) : IRenderNode {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Render(IRenderTreeBuilder renderTree) {
            renderTree.Text("typeof(");
            instance.RenderFullName(renderTree);
            renderTree.Text(")");
        }
    }

    public readonly struct TypeFullNameRenderNode(IQtType instance) : IRenderNode {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Render(IRenderTreeBuilder renderTree) {
            instance.RenderFullName(renderTree);
        }
    }

    extension(AccessModifier modifier) {
        public string List => modifier.AsCSharpString();
    }

    extension(IQtType type) {
        public TypeExpressionRenderNode Expression => new(type);
        public TypeOfRenderNode TypeOf => new(type);
        public TypeFullNameRenderNode FullName => new(type);
    }
}
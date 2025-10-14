using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt;

internal static class SyntaxRendererExtensions {

    extension(AccessModifier modifier) {
        public string List => modifier.AsCSharpString();
    }

    extension(IQtType type) {
        public RenderNode<IQtType> Expression => new(type, static (renderTree, type) => type.RenderExpression(renderTree));

        public RenderNode<IQtType> TypeOf => new(type, static (renderTree, type) => {
            renderTree.Text("typeof(");
            type.RenderFullName(renderTree);
            renderTree.Text(")");
        });

        public RenderNode<IQtType> FullName => new(type, static (renderTree, type) => {
            type.RenderFullName(renderTree);
        });
    }

    extension(IRenderTreeBuilder renderTree) {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NewLine() {
            renderTree.Text("\n");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Line(string line) {
            renderTree.Text(line);
            renderTree.NewLine();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartCodeBlock() {
            renderTree.Line("{");
            renderTree.StartBlock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndCodeBlock() {
            renderTree.EndBlock();
            renderTree.Line("}");
        }

        public void SeparatedList<TItem>(ReadOnlySpan<TItem> items) where TItem : IRenderNode {
            for (var i = 0; i < items.Length; i++) {
                if (i > 0) {
                    renderTree.Text(", ");
                }

                renderTree.Node(items[i]);
            }
        }

        public void SeparatedList<TItem, TRenderItem>(ReadOnlySpan<TItem> items, Func<TItem, TRenderItem> renderItemSelector) where TRenderItem : IRenderNode {
            for (var i = 0; i < items.Length; i++) {
                if (i > 0) {
                    renderTree.Text(", ");
                }

                var item = renderItemSelector(items[i]);
                renderTree.Node(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QualifiedTypeName(Type type) {
            RuntimeTypeSerializer.RenderInto(renderTree, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TypeOf(Type type) {
            renderTree.Text("typeof(");
            RuntimeTypeSerializer.RenderInto(renderTree, type);
            renderTree.Text(")");
        }
    }
}
using System.Runtime.CompilerServices;
using Mumei.CodeGen.Playground;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt;

internal static class SyntaxRendererExtensions {

    extension(AccessModifier modifier) {
        public string List => modifier.AsCSharpString();
    }

    extension(ParameterAttributes attributes) {
        public RenderNode<ParameterAttributes> List => new(attributes, (tree, parameterAttributes) => {
            if (parameterAttributes == ParameterAttributes.None) {
                return;
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.This)) {
                tree.Text("this ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.Ref)) {
                tree.Text("ref ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.Out)) {
                tree.Text("out ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.In)) {
                tree.Text("in ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.Params)) {
                tree.Text("params ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.Readonly)) {
                tree.Text("readonly ");
            }
        });
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
using System.Runtime.CompilerServices;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Rendering;

public static class SyntaxRendererExtensions {
    extension(AccessModifierList modifiers) {
        public string List => modifiers.AsCSharpString();
    }

    extension(ParameterAttributes attributes) {
        public RenderFragment<ParameterAttributes> List => new(attributes, (tree, parameterAttributes) => {
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

        public void SeparatedList<TItem>(ReadOnlySpan<TItem> items) where TItem : IRenderFragment {
            for (var i = 0; i < items.Length; i++) {
                if (i > 0) {
                    renderTree.Text(", ");
                }

                renderTree.Node(items[i]);
            }
        }

        public void List<TItem>(ReadOnlySpan<TItem> items) where TItem : IRenderFragment {
            for (var i = 0; i < items.Length; i++) {
                var t = items[i];
                renderTree.Node(t);
                var isLast = i == items.Length - 1;
                if (!isLast) {
                    renderTree.NewLine();
                }
            }
        }

        public void SeparatedList<TItem, TRenderItem>(ReadOnlySpan<TItem> items, Func<TItem, TRenderItem> renderItemSelector) where TRenderItem : IRenderFragment {
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

        public void QualifiedTypeName<T>() {
            RuntimeTypeSerializer.RenderInto(renderTree, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TypeOf(Type type) {
            renderTree.Text("typeof(");
            RuntimeTypeSerializer.RenderInto(renderTree, type);
            renderTree.Text(")");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // Rider doesn't seem to be able to recognize InterpolatedStringHandlerArgument attribute targets
    // in extension blocks, use a regular extension method instead.
    public static void InterpolatedLine(this IRenderTreeBuilder renderTree, [InterpolatedStringHandlerArgument(nameof(renderTree))] IRenderTreeBuilder.InterpolatedStringHandler line) {
        renderTree.Interpolate(line);
        renderTree.NewLine();
    }
}
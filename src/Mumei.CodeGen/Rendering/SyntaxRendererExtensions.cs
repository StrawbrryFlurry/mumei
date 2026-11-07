using System.Runtime.CompilerServices;

namespace Mumei.CodeGen.Rendering;

public static class SyntaxRendererExtensions {
    extension(IRenderTreeBuilder renderTree) {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Line(string line) {
            renderTree.Text(line);
            renderTree.NewLine();
        }

        /// <summary>Writes multi-line text, indenting every non-empty line.</summary>
        public void Lines(string text) {
            foreach (var line in CodeBuilder.SplitLines(text)) {
                if (!string.IsNullOrWhiteSpace(line)) {
                    renderTree.Text(line);
                }

                renderTree.NewLine();
            }
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
            foreach (var t in items) {
                renderTree.Node(t);
            }
        }

        public void MemberList<TItem>(ReadOnlySpan<TItem> items) where TItem : IRenderFragment {
            for (var i = 0; i < items.Length; i++) {
                renderTree.Node(items[i]);
                if (i < items.Length - 1) {
                    renderTree.NewLine();
                }
            }
        }

        public void MemberList<TItem>(ReadOnlySpan<TItem> items, ref bool hadPreviousMember) where TItem : IRenderFragment {
            if (items.IsEmpty) {
                return;
            }

            if (hadPreviousMember) {
                renderTree.NewLine();
            }

            hadPreviousMember = true;
            for (var i = 0; i < items.Length; i++) {
                renderTree.Node(items[i]);
                if (i < items.Length - 1) {
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
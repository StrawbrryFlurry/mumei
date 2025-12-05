using System.Runtime.CompilerServices;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Rendering;

public static class SyntaxRendererExtensions {
    extension(AccessModifierList modifiers) {
        public string List => modifiers.AsCSharpString();
        public RenderFragment<AccessModifierList> RenderAsExpression => new(modifiers, (renderTree, accessModifiers) => {
            for (var i = 0; i < accessModifiers.Modifiers.Length; i++) {
                var accessModifiersModifier = accessModifiers.Modifiers[i];
                if (!accessModifiersModifier.HasValue) {
                    continue;
                }

                var factoryName = accessModifiersModifier.Value switch {
                    "public" => "Public",
                    "private" => "Private",
                    "abstract" => "Abstract",
                    "protected" => "Protected",
                    "internal" => "Internal",
                    "file" => "File",
                    "sealed" => "Sealed",
                    "readonly" => "Readonly",
                    "static" => "Static",
                    "partial" => "Partial",
                    "virtual" => "Virtual",
                    "override" => "Override",
                    "async" => "Async",
                    _ => throw new ArgumentOutOfRangeException()
                };

                renderTree.Interpolate($"{typeof(AccessModifier)}.{factoryName}");
                if (i < accessModifiers.Modifiers.Length - 1) {
                    renderTree.Text(" + ");
                }
            }
        });
    }

    extension(ParameterAttributes attributes) {
        public RenderFragment<ParameterAttributes> List => new(attributes, (renderTree, parameterAttributes) => {
            if (parameterAttributes == ParameterAttributes.None) {
                return;
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.This)) {
                renderTree.Text("this ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.Ref)) {
                renderTree.Text("ref ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.Out)) {
                renderTree.Text("out ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.In)) {
                renderTree.Text("in ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.Params)) {
                renderTree.Text("params ");
            }

            if (parameterAttributes.HasFlag(ParameterAttributes.Readonly)) {
                renderTree.Text("readonly ");
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
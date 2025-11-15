using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

internal static class QtRepresentable {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArrayRepresentable<DefaultMemoryAccessor<TElement>, TElement, TRepresentable> RepresentAsQtArray<TElement, TRepresentable>(
        this ReadOnlyMemory<TElement> memory,
        Func<TElement, TRepresentable> representationSelector
    ) where TRepresentable : ISyntaxRepresentable {
        return RepresentAsQtArray(memory.AsMemoryAccessor(), representationSelector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArrayRepresentable<TMemeoryAccessor, TElement, TRepresentable> RepresentAsQtArray<TMemeoryAccessor, TElement, TRepresentable>(
        this TMemeoryAccessor state,
        Func<TElement, TRepresentable> representationSelector
    ) where TMemeoryAccessor : IQtMemoryAccessor<TElement> where TRepresentable : ISyntaxRepresentable {
        return new ArrayRepresentable<TMemeoryAccessor, TElement, TRepresentable>(state, representationSelector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SeparatedListRepresentable<DefaultMemoryAccessor<TElement>, TElement, TRepresentable> RepresentAsSeparatedList<TElement, TRepresentable>(
        this Memory<TElement> memory,
        Func<TElement, TRepresentable> representationSelector
    ) where TRepresentable : ISyntaxRepresentable {
        return RepresentAsSeparatedList(memory.AsMemoryAccessor(), representationSelector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SeparatedListRepresentable<DefaultMemoryAccessor<TElement>, TElement, TRepresentable> RepresentAsSeparatedList<TElement, TRepresentable>(
        this ReadOnlyMemory<TElement> memory,
        Func<TElement, TRepresentable> representationSelector
    ) where TRepresentable : ISyntaxRepresentable {
        return RepresentAsSeparatedList(memory.AsMemoryAccessor(), representationSelector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SeparatedListRepresentable<TMemoryAccessor, TElement, TRepresentable> RepresentAsSeparatedList<TMemoryAccessor, TElement, TRepresentable>(
        this TMemoryAccessor state,
        Func<TElement, TRepresentable> representationSelector
    ) where TMemoryAccessor : IQtMemoryAccessor<TElement> where TRepresentable : ISyntaxRepresentable {
        return new SeparatedListRepresentable<TMemoryAccessor, TElement, TRepresentable>(state, representationSelector);
    }

    public static DefaultMemoryAccessor<TElement> AsMemoryAccessor<TElement>(in this Memory<TElement> memory) {
        return new DefaultMemoryAccessor<TElement>(memory);
    }

    public static DefaultMemoryAccessor<TElement> AsMemoryAccessor<TElement>(in this ReadOnlyMemory<TElement> memory) {
        var writableMemory = MemoryMarshal.AsMemory(memory);
        return new DefaultMemoryAccessor<TElement>(writableMemory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DefaultStringRepresentable Qt(this string s) {
        return new DefaultStringRepresentable(s);
    }
}

internal readonly struct DefaultStringRepresentable(
    string s
) : ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write(s);
    }
}

internal readonly struct ArrayRepresentable<TState, TElement, TRepresentable>(
    TState state,
    Func<TElement, TRepresentable> representationSelector
) : ISyntaxRepresentable where TState : IQtMemoryAccessor<TElement> where TRepresentable : ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write("[ ");
        var listRepresenter = new SeparatedListRepresentable<TState, TElement, TRepresentable>(state, representationSelector);
        writer.Write(listRepresenter, format);
        writer.Write(" ]");
    }
}

// ReSharper disable once InconsistentNaming
internal readonly struct SeparatedListRepresentable<TState, TElement, TRepresentable>(
    TState state,
    Func<TElement, TRepresentable> representationSelector
) : ISyntaxRepresentable where TState : IQtMemoryAccessor<TElement> where TRepresentable : ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        var span = state.Memory.Span;
        for (var i = 0; i < span.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            var item = span[i];
            var representable = representationSelector(item);
            writer.Write(representable, format);
        }
    }
}

internal static class QtRepresentableRendererExtensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SeparatedListRenderFragment<DefaultMemoryAccessor<TElement>, TElement, TRenderNode> RenderAsSeparatedList<TElement, TRenderNode>(
        this Memory<TElement> memory,
        Func<TElement, TRenderNode> representationSelector
    ) where TRenderNode : IRenderFragment {
        return RenderAsSeparatedList(memory.AsMemoryAccessor(), representationSelector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SeparatedListRenderFragment<DefaultMemoryAccessor<TElement>, TElement, TRenderNode> RenderAsSeparatedList<TElement, TRenderNode>(
        this ReadOnlyMemory<TElement> memory,
        Func<TElement, TRenderNode> representationSelector
    ) where TRenderNode : IRenderFragment {
        return RenderAsSeparatedList(memory.AsMemoryAccessor(), representationSelector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SeparatedListRenderFragment<TMemoryAccessor, TElement, TRenderNode> RenderAsSeparatedList<TMemoryAccessor, TElement, TRenderNode>(
        this TMemoryAccessor state,
        Func<TElement, TRenderNode> representationSelector
    ) where TMemoryAccessor : IQtMemoryAccessor<TElement> where TRenderNode : IRenderFragment {
        return new SeparatedListRenderFragment<TMemoryAccessor, TElement, TRenderNode>(state, representationSelector);
    }
}

internal readonly struct SeparatedListRenderFragment<TState, TElement, TRenderNode>(
    TState state,
    Func<TElement, TRenderNode> nodeSelector
) : IRenderFragment where TState : IQtMemoryAccessor<TElement> where TRenderNode : IRenderFragment {
    public void Render(IRenderTreeBuilder renderer) {
        var span = state.Memory.Span;
        for (var i = 0; i < span.Length; i++) {
            if (i > 0) {
                renderer.Text(", ");
            }

            var item = span[i];
            var representable = nodeSelector(item);
            renderer.Node(representable);
        }
    }
}

internal readonly struct DefaultMemoryAccessor<T>(
    Memory<T> memory
) : IQtMemoryAccessor<T> {
    public Memory<T> Memory => memory;
}
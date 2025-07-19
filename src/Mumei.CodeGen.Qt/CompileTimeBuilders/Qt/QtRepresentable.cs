using System.Collections.Immutable;
using Mumei.CodeGen.Qt.Output;

namespace Mumei.CodeGen.Qt.Qt;

internal static class QtRepresentable {
    public static ArrayRepresentable<TState, TRepresentable> RepresentAsQtArray<TState, TRepresentable>(
        this TState[] state,
        Func<TState, TRepresentable> representationSelector
    ) where TRepresentable : ISyntaxRepresentable {
        return new ArrayRepresentable<TState, TRepresentable>(state, representationSelector);
    }

    public static ArrayRepresentable_ImmutableArray<TState, TRepresentable> RepresentAsQtArray<TState, TRepresentable>(
        this in ImmutableArray<TState> state,
        Func<TState, TRepresentable> representationSelector
    ) where TRepresentable : ISyntaxRepresentable {
        return new ArrayRepresentable_ImmutableArray<TState, TRepresentable>(state, representationSelector);
    }

    public static SeparatedListRepresentable<TState, TRepresentable> RepresentAsSeparatedList<TState, TRepresentable>(
        this TState[] state,
        Func<TState, TRepresentable> representationSelector
    ) where TRepresentable : ISyntaxRepresentable {
        return new SeparatedListRepresentable<TState, TRepresentable>(state, representationSelector);
    }

    public static SeparatedListRepresentable_ImmutableArray<TState, TRepresentable> RepresentAsSeparatedList<TState, TRepresentable>(
        this in ImmutableArray<TState> state,
        Func<TState, TRepresentable> representationSelector
    ) where TRepresentable : ISyntaxRepresentable {
        return new SeparatedListRepresentable_ImmutableArray<TState, TRepresentable>(state, representationSelector);
    }

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

// ReSharper disable once InconsistentNaming
internal readonly struct ArrayRepresentable_ImmutableArray<TState, TRepresentable>(
    ImmutableArray<TState> state,
    Func<TState, TRepresentable> representationSelector
) : ISyntaxRepresentable where TRepresentable : ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write("[ ");
        var listRepresenter = new SeparatedListRepresentable_ImmutableArray<TState, TRepresentable>(state, representationSelector);
        writer.Write(listRepresenter, format);
        writer.Write(" ]");
    }
}

internal readonly struct ArrayRepresentable<TState, TRepresentable>(
    TState[] state,
    Func<TState, TRepresentable> representationSelector
) : ISyntaxRepresentable where TRepresentable : ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        writer.Write("[ ");
        var listRepresenter = new SeparatedListRepresentable<TState, TRepresentable>(state, representationSelector);
        writer.Write(listRepresenter, format);
        writer.Write(" ]");
    }
}

// ReSharper disable once InconsistentNaming
internal readonly struct SeparatedListRepresentable_ImmutableArray<TState, TRepresentable>(
    ImmutableArray<TState> state,
    Func<TState, TRepresentable> representationSelector
) : ISyntaxRepresentable where TRepresentable : ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        for (var i = 0; i < state.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            var item = state[i];
            var representable = representationSelector(item);
            writer.Write(representable, format);
        }
    }
}

// ReSharper disable once InconsistentNaming
internal readonly struct SeparatedListRepresentable<TState, TRepresentable>(
    TState[] state,
    Func<TState, TRepresentable> representationSelector
) : ISyntaxRepresentable where TRepresentable : ISyntaxRepresentable {
    public void WriteSyntax<TSyntaxWriter>(ref TSyntaxWriter writer, string? format = null) where TSyntaxWriter : ISyntaxWriter {
        for (var i = 0; i < state.Length; i++) {
            if (i > 0) {
                writer.Write(", ");
            }

            var item = state[i];
            var representable = representationSelector(item);
            writer.Write(representable, format);
        }
    }
}
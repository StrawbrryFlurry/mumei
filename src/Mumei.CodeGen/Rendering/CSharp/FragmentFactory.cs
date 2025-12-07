namespace Mumei.CodeGen.Rendering.CSharp;

public static class FragmentFactory {
    public static LocalFragment Var(
        ExpressionFragment name,
        out ExpressionFragment localName
    ) {
        localName = name;
        var local = new LocalFragment(TypeInfoFragment.Var, name);
        return local;
    }

    public static LiteralFragment<T> Literal<T>(T value) {
        return new LiteralFragment<T>(value);
    }

    public static StringLiteralFragment StringLiteral(string value) {
        return new StringLiteralFragment(value);
    }

    public static RawStringLiteralFragment RawStringLiteral(string value, string quotes = Strings.RawStringLiteral9) {
        return new RawStringLiteralFragment(value, quotes);
    }
}

public readonly struct LiteralFragment<T>(T value) : IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Value(value);
        renderTree.Text(value?.ToString() ?? "null");
    }

    public override string ToString() {
        return DebugRenderer.Render(this);
    }
}

public readonly struct StringLiteralFragment(string value) : IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"\"{value}\"");
    }

    public override string ToString() {
        return DebugRenderer.Render(this);
    }
}

public readonly struct RawStringLiteralFragment(string value, string quotes) : IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Block(
            $"""
             {quotes}
             {value}
             {quotes}
             """
        );
    }

    public override string ToString() {
        return DebugRenderer.Render(this);
    }
}
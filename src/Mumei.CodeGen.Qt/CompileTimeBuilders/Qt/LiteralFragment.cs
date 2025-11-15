namespace Mumei.CodeGen.Qt;

public static class LiteralNode {
    public static LiteralFragment<T> For<T>(T value) {
        return new LiteralFragment<T>(value);
    }

    public static StringLiteralFragment ForString(string value) {
        return new StringLiteralFragment(value);
    }

    public static RawStringLiteralFragment ForRawString(string value, string quotes = Strings.RawStringLiteral9) {
        return new RawStringLiteralFragment(value, quotes);
    }
}

public readonly struct LiteralFragment<T>(T value) : IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Value(value);
        renderTree.Text(value?.ToString() ?? "null");
    }
}

public readonly struct StringLiteralFragment(string value) : IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"\"{value}\"");
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
}
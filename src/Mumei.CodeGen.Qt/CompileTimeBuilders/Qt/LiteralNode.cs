namespace Mumei.CodeGen.Qt;

public static class LiteralNode {
    public static LiteralNode<T> For<T>(T value) {
        return new LiteralNode<T>(value);
    }

    public static StringLiteralNode ForString(string value) {
        return new StringLiteralNode(value);
    }

    public static RawStringLiteralNode ForRawString(string value, string quotes = Strings.RawStringLiteral9) {
        return new RawStringLiteralNode(value, quotes);
    }
}

public readonly struct LiteralNode<T>(T value) : IRenderNode {
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Value(value);
        renderTree.Text(value?.ToString() ?? "null");
    }
}

public readonly struct StringLiteralNode(string value) : IRenderNode {
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Interpolate($"\"{value}\"");
    }
}

public readonly struct RawStringLiteralNode(string value, string quotes) : IRenderNode {
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
namespace Mumei.CodeGen.Qt.Output;

public static class SyntaxWriterExtensions {
    public static string ToSyntaxInternal<TRepresentable>(this TRepresentable syntax) where TRepresentable : ISyntaxRepresentable {
        var writer = new SyntaxWriter();
        syntax.WriteSyntax(ref writer);
        return writer.ToSyntax();
    }
}
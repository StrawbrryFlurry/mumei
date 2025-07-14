namespace Mumei.CodeGen.Qt.Output;

public static class SyntaxWriterExtensions {
    public static string ToSyntaxInternal(this ISyntaxRepresentable syntax) {
        var writer = new SyntaxWriter();
        syntax.WriteSyntax(writer);
        return writer.ToSyntax();
    }
}
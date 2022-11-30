using Mumei.CodeGen.SyntaxNodes;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Tests.Utils;

public static class SyntaxWriterUtilities {
  public static string WriteSyntaxAsString(this Syntax syntax) {
    var writer = new TypeAwareSyntaxWriter(new SyntaxTypeContext());
    syntax.WriteAsSyntax(writer);

    return writer.ToSyntax();
  }

  public static string WriteSyntaxAsString(this Syntax syntax, out SyntaxTypeContext ctx) {
    ctx = new SyntaxTypeContext();
    var writer = new TypeAwareSyntaxWriter(ctx);
    syntax.WriteAsSyntax(writer);

    return writer.ToSyntax();
  }
}
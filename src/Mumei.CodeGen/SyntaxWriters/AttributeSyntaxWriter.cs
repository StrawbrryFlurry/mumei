using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxWriters;

public class AttributeSyntaxWriter : TypeAwareSyntaxWriter {
  public AttributeSyntaxWriter(WriterTypeContext ctx) : base(ctx) {
  }

  public AttributeSyntaxWriter(int indentLevel, WriterTypeContext ctx) : base(indentLevel, ctx) {
  }

  public void WriteAttribute(Type attributeType, params object[] arguments) {
    IncludeTypeNamespace(attributeType);
    WriteLineStart($"[{attributeType.GetAttributeName()}(");

    if (arguments.Any()) {
      WriteAttributeArguments(arguments);
    }

    WriteLineEnd(")]");
  }

  private void WriteAttributeArguments(object[] arguments) {
    var argumentString = arguments.Select(ConvertValueToStringRepresentation).JoinBy(", ");
    Write(argumentString);
  }
}
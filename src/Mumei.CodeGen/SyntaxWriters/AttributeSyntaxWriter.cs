using Mumei.CodeGen.Extensions;
using Mumei.CodeGen.SyntaxNodes;

namespace Mumei.CodeGen.SyntaxWriters;

public class AttributeSyntaxWriter : TypeAwareSyntaxWriter {
  public AttributeSyntaxWriter(SyntaxTypeContext? ctx) : base(ctx) {
  }

  public void WriteAttributes(AttributeUsage[] attributes, bool sameLine = false) {
    var separator = sameLine ? " " : NewLine;

    for (var i = 0; i < attributes.Length; i++) {
      var attribute = attributes[i];
      WriteAttribute(attribute);

      var shouldNotAppendSpace = i == attributes.Length - 1 && sameLine;

      if (!shouldNotAppendSpace) {
        Write(separator);
      }
    }
  }

  public void WriteAttribute(AttributeUsage attribute) {
    var attributeType = attribute.Type;
    var positionalArguments = attribute.Arguments;
    var namedArguments = attribute.NamedArguments;

    IncludeTypeNamespace(attributeType);
    WriteLineStart($"[{attributeType.GetAttributeName()}(");

    var hasPositionalArguments = positionalArguments.Any();

    if (hasPositionalArguments) {
      WritePositionalAttributeArguments(positionalArguments);
    }

    if (namedArguments.Any()) {
      if (hasPositionalArguments) {
        Write(", ");
      }

      WriteNamedAttributeArguments(namedArguments);
    }

    Write(")]");
  }

  private void WritePositionalAttributeArguments(object[] arguments) {
    var argumentString = arguments.Select(GetValueAsExpressionSyntax).JoinBy(", ");
    Write(argumentString);
  }

  private void WriteNamedAttributeArguments(Dictionary<NamedAttributeParameter, object> arguments) {
    var argumentString = arguments.Select(e => {
      var parameter = e.Key;
      var separator = GetNamedParameterSeparator(parameter);
      var value = GetValueAsExpressionSyntax(e.Value);

      return $"{parameter.Name}{separator} {value}";
    }).JoinBy(", ");

    Write(argumentString);
  }

  private string GetNamedParameterSeparator(NamedAttributeParameter parameter) {
    return parameter.IsField ? " =" : ":";
  }
}
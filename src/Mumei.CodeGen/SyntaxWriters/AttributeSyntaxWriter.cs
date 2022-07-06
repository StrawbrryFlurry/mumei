using System.Runtime.CompilerServices;
using Mumei.CodeGen.Extensions;
using Mumei.CodeGen.SyntaxBuilders;

namespace Mumei.CodeGen.SyntaxWriters;

public class AttributeSyntaxWriter : TypeAwareSyntaxWriter {
  public AttributeSyntaxWriter(SyntaxTypeContext ctx) : base(ctx) {
  }

  public AttributeSyntaxWriter(int indentLevel, SyntaxTypeContext ctx) : base(indentLevel, ctx) {
  }

  [StateMachine(typeof(string))]
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

    WriteLineEnd(")]");
  }

  private void WritePositionalAttributeArguments(object[] arguments) {
    var argumentString = arguments.Select(ConvertExpressionValueToSyntax).JoinBy(", ");
    Write(argumentString);
  }

  private void WriteNamedAttributeArguments(Dictionary<NamedAttributeParameter, object> arguments) {
    var argumentString = arguments.Select(e => {
      var parameter = e.Key;
      var separator = GetNamedParameterSeparator(parameter);
      var value = ConvertExpressionValueToSyntax(e.Value);

      return $"{parameter.Name}{separator} {value}";
    }).JoinBy(", ");

    Write(argumentString);
  }

  private string GetNamedParameterSeparator(NamedAttributeParameter parameter) {
    return parameter.IsField ? " =" : ":";
  }
}
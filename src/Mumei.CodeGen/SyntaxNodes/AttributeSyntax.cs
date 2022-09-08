using Mumei.CodeGen.Extensions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class AttributeSyntax : Syntax {
  public Dictionary<NamedAttributeParameter, object> NamedArguments = new();
  public object[] PositionalArguments = Array.Empty<object>();
  public Type Type;

  public AttributeSyntax(Type type) {
    Type = type;
  }

  public AttributeSyntax(Type type, Syntax parent) : base(parent) {
    Type = type;
  }

  public static AttributeSyntax Create<TAttribute>() {
    return new AttributeSyntax(typeof(TAttribute));
  }

  public static AttributeSyntax Create<TAttribute>(params object[] positionalArguments) {
    return new AttributeSyntax(typeof(TAttribute)) {
      PositionalArguments = positionalArguments
    };
  }

  public static AttributeSyntax Create<TAttribute>(
    Dictionary<NamedAttributeParameter, object> namedArguments,
    params object[] positionalArguments
  ) {
    return new AttributeSyntax(typeof(TAttribute)) {
      PositionalArguments = positionalArguments,
      NamedArguments = namedArguments
    };
  }

  // [<TypeName>(...Arguments)]
  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.TypeContext.IncludeTypeNamespace(Type);

    writer.WriteLineStart("[");
    writer.Write(Type.GetAttributeName());
    writer.Write("(");

    WriteArguments(writer);

    writer.Write(")");
    writer.WriteLineStart("]");
  }

  public override Syntax Clone() {
    return new AttributeSyntax(Type) {
      NamedArguments = NamedArguments,
      PositionalArguments = PositionalArguments
    };
  }

  private void WriteArguments(ITypeAwareSyntaxWriter writer) {
    for (var i = 0; i < PositionalArguments.Length; i++) {
      var argument = PositionalArguments[i];
      WritePositionalArgument(writer, argument, i);
    }

    for (var i = 0; i < NamedArguments.Count; i++) {
      var argument = NamedArguments.ElementAt(i);
      WriteNamedArgument(writer, argument, i);
    }
  }

  private void WritePositionalArgument(ITypeAwareSyntaxWriter writer, object argument, int index) {
    writer.WriteValueAsExpressionSyntax(argument);
    var hasNextArgument = index < PositionalArguments.Length - 1 || NamedArguments.Count > 0;

    if (hasNextArgument) {
      writer.Write(", ");
    }
  }

  private void WriteNamedArgument(
    ITypeAwareSyntaxWriter writer,
    KeyValuePair<NamedAttributeParameter, object> argument,
    int argumentIndex
  ) {
    var parameter = argument.Key;
    writer.Write(parameter.Name);
    writer.Write(GetNamedParameterSeparator(parameter));

    writer.Write(" ");

    writer.WriteValueAsExpressionSyntax(argument.Value);

    if (argumentIndex < NamedArguments.Count - 1) {
      writer.Write(", ");
    }
  }

  private string GetNamedParameterSeparator(NamedAttributeParameter parameter) {
    return parameter.IsField ? " =" : ":";
  }
}

public struct NamedAttributeParameter {
  public string Name;

  /// <summary>
  ///   When true uses `[Attribute(Name = "Name")]` instead of `[Attribute(name: "Name")]`
  /// </summary>
  public bool IsField;

  public static implicit operator NamedAttributeParameter(string name) {
    return new NamedAttributeParameter {
      Name = name
    };
  }
}
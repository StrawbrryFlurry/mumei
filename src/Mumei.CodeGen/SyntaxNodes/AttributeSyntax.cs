using Mumei.CodeGen.Extensions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class AttributeSyntax : Syntax {
  public object[] Arguments = Array.Empty<object>();
  public Dictionary<NamedAttributeParameter, object> NamedArguments = new();
  public Type Type;

  public AttributeSyntax(Type type) : base(type.Name) {
    Type = type;
  }

  public AttributeSyntax(Type type, Syntax parent) : base(type.Name, parent) {
    Type = type;
  }

  public static AttributeSyntax Create<TAttribute>() {
    return new AttributeSyntax(typeof(TAttribute));
  }

  public static AttributeSyntax Create<TAttribute>(params object[] arguments) {
    return new AttributeSyntax(typeof(TAttribute)) {
      Arguments = arguments
    };
  }

  public static AttributeSyntax Create<TAttribute>(
    Dictionary<NamedAttributeParameter, object> namedArguments,
    params object[] arguments
  ) {
    return new AttributeSyntax(typeof(TAttribute)) {
      Arguments = arguments,
      NamedArguments = namedArguments
    };
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.WriteLineStart("[");
    writer.Write(Type.GetAttributeName());
    writer.Write("(");

    foreach (var argument in Arguments) writer.WriteValueAsExpressionSyntax(argument);

    writer.Write(")");
    writer.WriteLineStart("]");
  }
}
using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class VariableExpressionSyntax<T> : ExpressionSyntax {
  public VariableExpressionSyntax(string name, Syntax? parent = null)
    : base(Expression.Variable(typeof(T), name), parent) { }

  public T Value { get; set; } = default!;
}

public class VariableDeclarationSyntax : StatementSyntax {
  public readonly string Identifier;
  public readonly ExpressionSyntax? Initializer;
  public readonly Type Type;

  public VariableDeclarationSyntax(
    Type type,
    string identifier,
    ExpressionSyntax? initializer = null,
    Syntax? parent = null
  ) : base(parent) {
    Initializer = initializer;
    Identifier = identifier;
    Type = type;
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.WriteTypeName(Type);
    writer.Write(" ");
    writer.Write(Identifier);

    if (Initializer is not null) {
      WriteInitializer(writer);
    }

    writer.Write(";");
  }

  private void WriteInitializer(ITypeAwareSyntaxWriter writer) {
    writer.Write(" = ");
    Initializer!.WriteAsSyntax(writer);
  }
}
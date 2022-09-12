using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class VariableExpressionSyntax : VariableExpressionSyntax<object> {
  public readonly Type Type;

  public VariableExpressionSyntax(Type type, string name, Syntax? parent = null) : base(name, parent) {
    Type = type;
  }

  public override Syntax Clone() {
    return new VariableExpressionSyntax(Type, Identifier);
  }
}

public class VariableExpressionSyntax<T> : ExpressionSyntax, IValueHolderSyntax<T> {
  public VariableExpressionSyntax(string name, Syntax? parent = null)
    : base(Expression.Variable(typeof(T), name), parent) {
    Identifier = name;
  }

  public string Identifier { get; }
  public T Value { get; set; } = default!;

  public override Syntax Clone() {
    return new VariableExpressionSyntax<T>(Identifier);
  }
}

public class VariableDeclarationStatementSyntax : StatementSyntax, IValueHolderDeclarationSyntax {
  public readonly string Identifier;

  public VariableDeclarationStatementSyntax(
    Type type,
    string identifier,
    ExpressionSyntax? initializer = null,
    Syntax? parent = null
  ) : base(parent) {
    Initializer = initializer;
    Identifier = identifier;
    Type = type;
  }

  public ExpressionSyntax? Initializer { get; }
  public Type Type { get; }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.WriteTypeName(Type);
    writer.Write(" ");
    writer.Write(Identifier);

    if (Initializer is not null) {
      WriteInitializer(writer);
    }

    writer.Write(";");
  }

  public override Syntax Clone() {
    var initializer = Initializer?.Clone() as ExpressionSyntax;
    return new VariableDeclarationStatementSyntax(Type, Identifier, initializer);
  }

  private void WriteInitializer(ITypeAwareSyntaxWriter writer) {
    writer.Write(" = ");
    Initializer!.WriteAsSyntax(writer);
  }
}
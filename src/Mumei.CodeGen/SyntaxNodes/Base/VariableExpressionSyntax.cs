using System.Linq.Expressions;
using System.Reflection;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class VariableExpressionSyntax<T> : ExpressionSyntax, ITransformMemberExpression {
  public readonly string Name;

  public VariableExpressionSyntax(string name, Syntax? parent = null)
    : base(Expression.Variable(typeof(T), name), parent) {
    Name = name;
  }

  public T Value { get; set; } = default!;

  public Expression TransformMemberAccess(Expression target, MemberInfo member) {
    // We allow users to imitate variable access though the "Value"
    // property. To reflect that in the generated expression, we need to
    // replace the current target with the variable instance.
    if (member.Name == nameof(Value)) {
      return ExpressionNode;
    }

    return Expression.MakeMemberAccess(target, member);
  }
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
using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class FieldSyntax : FieldSyntax<object> {
  public FieldSyntax(Type type, string identifier, Syntax parent) : base(type, identifier, parent) { }
}

public class FieldSyntax<T> : MemberSyntax, IValueHolderSyntax<T>, IValueHolderDeclarationSyntax {
  public FieldSyntax(string identifier, Syntax parent) : base(typeof(T), identifier, parent) { }
  protected FieldSyntax(Type type, string identifier, Syntax parent) : base(type, identifier, parent) { }

  public override int Priority => 0;

  public ExpressionSyntax? Initializer { get; set; }

  public T? Value { get; set; }

  public void SetInitialValue(object? value) {
    Initializer = Expression.Constant(value);
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    if (HasAttributes) {
      WriteAttributes(writer);
      writer.WriteLine();
    }

    if (Type is null) {
      throw new InvalidOperationException("Field type cannot be null");
    }

    WriteVisibility(writer);

    writer.WriteTypeName(Type);
    writer.Write(" ");
    writer.Write(Identifier);

    if (Initializer is not null) {
      writer.Write(" = ");
      writer.WriteValueAsExpressionSyntax(Initializer);
    }

    writer.WriteLineEnd(";");
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }
}
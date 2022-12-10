using System.Linq.Expressions;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public sealed class FieldSyntax : FieldSyntax<object> {
  public FieldSyntax(Type type, string identifier, Syntax parent) : base(type, identifier, parent) { }
}

public class FieldSyntax<T> : MemberSyntax, IMemberValueHolderSyntax<T>, IMemberValueHolderDeclarationSyntax {
  private ExpressionSyntax? _initilizer;

  public FieldSyntax(string identifier, Syntax parent) : base(typeof(T), identifier, parent) { }
  protected FieldSyntax(Type type, string identifier, Syntax parent) : base(type, identifier, parent) { }

  protected internal override int Priority => 0;

  public ExpressionSyntax? Initializer {
    get => _initilizer;
    set {
      value?.SetParent(this);
      _initilizer = value;
    }
  }

  public T Value { get; set; } = default!;

  public void SetInitialValue(T? value) {
    Initializer = Expression.Constant(value);
  }

  public void SetInitialValue(Expression<Func<T>> value) {
    Initializer = value;
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
    var attributes = AttributeList.Clone<AttributeListSyntax>();

    var clone = new FieldSyntax<T>(Identifier, null!) {
      AttributeList = attributes,
      Initializer = Initializer,
      Value = Value,
      Visibility = Visibility
    };

    return clone;
  }
}
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class FieldSyntax : MemberSyntax {
  /// <summary>
  ///   The initial value of the field.
  ///   <remarks>
  ///     null is not explicitly set.
  ///   </remarks>
  /// </summary>
  public object? Initializer { get; set; }

  public override int Priority => 0;

  public FieldSyntax(string name, Syntax parent) : base(name, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    if (Type is null) {
      throw new InvalidOperationException("Field type cannot be null");
    }

    if (HasAttributes) {
      writer.Write(GetAttributeSyntax()!);
    }

    writer.Write(Visibility);

    writer.WriteTypeName(Type);
    writer.Write(" ");
    writer.Write(Name);

    if (Initializer is not null) {
      writer.Write(" = ");
      writer.WriteValueAsExpressionSyntax(Initializer);
    }

    writer.WriteLineEnd(";");
  }
}
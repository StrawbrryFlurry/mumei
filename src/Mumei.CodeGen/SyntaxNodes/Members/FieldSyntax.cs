using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class FieldSyntax : MemberSyntax {
  public FieldSyntax(string identifier, Syntax parent) : base(identifier, parent) { }

  /// <summary>
  ///   The initial value of the field.
  ///   <remarks>
  ///     null is not explicitly set.
  ///   </remarks>
  /// </summary>
  public object? Initializer { get; set; }

  public override int Priority => 0;

  public void SetInitialValue(object? value) {
    Initializer = value;
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    WriteAttributes(writer);

    if (Type is null) {
      throw new InvalidOperationException("Field type cannot be null");
    }

    writer.Write(Visibility);

    writer.WriteTypeName(Type);
    writer.Write(" ");
    writer.Write(Identifier);

    if (Initializer is not null) {
      writer.Write(" = ");
      writer.WriteValueAsExpressionSyntax(Initializer);
    }

    writer.WriteLineEnd(";");
  }
}
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class FieldSyntax : MemberSyntax {
  public override int Priority => 0;
  public object? Initializer { get; set; }

  public FieldSyntax(string name, Syntax parent) : base(name, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    if (Type is null) {
      throw new InvalidOperationException("Field type cannot be null");
    }

    var typeName = writer.GetTypeNameAsString(Type);
    writer.Write(typeName);
    writer.Write(" ");
    writer.Write(Name);
    writer.WriteLineEnd(";");
  }
}
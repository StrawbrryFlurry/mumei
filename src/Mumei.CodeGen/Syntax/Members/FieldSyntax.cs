using Mumei.CodeGen.Syntax;
using Mumei.CodeGen.SyntaxWriters;

// ReSharper disable once CheckNamespace
namespace Mumei.CodeGen.SyntaxBuilders;

public class FieldSyntax : MemberSyntax {
  public override int Priority => 0;
  public object? Initializer { get; set; }

  public FieldSyntax(MemberSyntaxConfiguration config) : base(config) {
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
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public enum PropertyAccessorType {
  Get,
  Set,
  Init
}

public class PropertyAccessor : TypeSyntax {
  public PropertyAccessor(PropertyAccessorType type, Syntax? parent) : base(type.ToString().ToLower(), parent) { }

  public PropertyAccessorType AccessorType { get; set; }
  public BlockSyntax Body { get; set; }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.Write(GetIdentifier());
    writer.Write(";");
  }

  public override Syntax Clone() {
    throw new NotImplementedException();
  }

  public override string GetIdentifier() {
    return AccessorType.ToString().ToLower();
  }
}
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public enum PropertyAccessorType {
  Get,
  Set,
  Init
}

public class PropertyAccessor : Syntax {
  public PropertyAccessorType AccessorType { get; set; }
  public BlockSyntax Body { get; set; }

  public PropertyAccessor(PropertyAccessorType type, Syntax? parent) : base(type.ToString().ToLower(), parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    writer.Write(GetIdentifier());
    writer.Write(";");
  }

  public override string GetIdentifier() {
    return AccessorType.ToString().ToLower();
  }
}
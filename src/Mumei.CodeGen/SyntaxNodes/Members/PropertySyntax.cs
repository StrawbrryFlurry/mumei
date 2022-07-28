using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class PropertySyntax : MemberSyntax {
  public PropertySyntax(string identifier, Syntax parent) : base(identifier, parent) { }

  public override int Priority => 1;

  public PropertyAccessor[] Accessors { get; set; }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
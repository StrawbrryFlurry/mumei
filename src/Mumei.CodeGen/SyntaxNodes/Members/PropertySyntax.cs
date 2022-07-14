using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class PropertySyntax : MemberSyntax {
  public override int Priority => 1;

  public PropertyAccessor[] Accessors { get; set; }

  public PropertySyntax(string identifier, Syntax parent) : base(identifier, parent) {
  }
  
  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class ClassSyntax : TypeSyntax {
  public ClassSyntax(string name) : base(name) {
  }

  public ClassSyntax(string name, Syntax? parent) : base(name, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
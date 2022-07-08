using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class ClassSyntax : TypeSyntax {
  public ClassSyntax(string identifier) : base(identifier) {
  }

  public ClassSyntax(string identifier, Syntax? parent) : base(identifier, parent) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
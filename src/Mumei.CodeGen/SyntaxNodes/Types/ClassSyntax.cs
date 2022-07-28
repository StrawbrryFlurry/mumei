using Mumei.CodeGen.SyntaxBuilders;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public class ClassDeclarationSyntax : TypeDeclarationSyntax {
  public ClassDeclarationSyntax(string identifier) : base(identifier) { }

  public ClassDeclarationSyntax(string identifier, Syntax? parent) : base(identifier, parent) { }

  public override AttributeListSyntax AttributeList { get; } = new(SeparationStrategy.NewLine);

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
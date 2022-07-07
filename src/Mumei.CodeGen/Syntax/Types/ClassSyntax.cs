using Mumei.CodeGen.Syntax;
using Mumei.CodeGen.SyntaxWriters;

// ReSharper disable once CheckNamespace
namespace Mumei.CodeGen.SyntaxBuilders;

public class ClassSyntax : TypeSyntax {
  public ClassSyntax(TypeSyntaxConfiguration config) : base(config) {
  }

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
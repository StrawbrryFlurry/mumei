using Mumei.CodeGen.SyntaxWriters;

// ReSharper disable once CheckNamespace
namespace Mumei.CodeGen.SyntaxBuilders;

public class ClassSyntax : TypeSyntax {
  public ClassSyntax(TypeSyntaxConfiguration config) : base(config) {
  }

  public override string WriteAsSyntax(TypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
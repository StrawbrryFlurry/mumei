using Mumei.CodeGen.Syntax;
using Mumei.CodeGen.SyntaxWriters;

// ReSharper disable once CheckNamespace
namespace Mumei.CodeGen.SyntaxBuilders;

public class FieldSyntax : MemberSyntax {
  public override int Priority => 0;

  public FieldSyntax(MemberSyntaxConfiguration config) : base(config) {
  }

  public override string WriteAsSyntax(TypeAwareSyntaxWriter writer) {
    throw new NotImplementedException();
  }
}
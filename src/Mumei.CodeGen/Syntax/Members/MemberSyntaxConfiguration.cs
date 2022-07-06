using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Syntax;

public class MemberSyntaxConfiguration : SyntaxConfiguration {
  public Type? Type;

  public MemberSyntaxConfiguration(string name, SyntaxTypeContext? typeContext = null) : base(name, typeContext) {
  }
}
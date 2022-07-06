using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Syntax;

public class TypeSyntaxConfiguration : SyntaxConfiguration {
  public Type[] TypeArguments = Type.EmptyTypes;

  public TypeSyntaxConfiguration(string name, SyntaxTypeContext? typeContext = null) : base(name, typeContext) {
  }
}
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Syntax;

public class SyntaxConfiguration {
  public AttributeUsage[] Attributes = Array.Empty<AttributeUsage>();
  public string Name;
  public SyntaxTypeContext TypeContext;
  public SyntaxVisibility Visibility = SyntaxVisibility.None;

  public SyntaxConfiguration(string name, SyntaxTypeContext? typeContext = null) {
    Name = name;
    TypeContext = typeContext ?? new SyntaxTypeContext();
  }
}
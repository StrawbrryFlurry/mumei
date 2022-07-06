using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxBuilders;

public abstract class Syntax {
  public string Name;
  public SyntaxVisibility Visibility;

  protected Syntax(SyntaxConfiguration config) {
    Name = config.Name;
    Visibility = config.Visibility;
  }

  public abstract string WriteAsSyntax(TypeAwareSyntaxWriter writer);
}
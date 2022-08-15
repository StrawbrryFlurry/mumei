using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class Syntax {
  public readonly Syntax? Parent;
  protected SyntaxTypeContext TypeContext;

  protected Syntax() {
    TypeContext = new SyntaxTypeContext();
  }

  protected Syntax(Syntax? parent) {
    Parent = parent;
    TypeContext = parent?.TypeContext ?? new SyntaxTypeContext();
  }

  public abstract void WriteAsSyntax(ITypeAwareSyntaxWriter writer);
}
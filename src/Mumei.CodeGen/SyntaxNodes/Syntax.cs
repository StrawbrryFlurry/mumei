using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class Syntax {
  protected SyntaxTypeContext TypeContext;

  protected Syntax() {
    TypeContext = new SyntaxTypeContext();
  }

  protected Syntax(Syntax? parent) {
    Parent = parent;
    TypeContext = parent?.TypeContext ?? new SyntaxTypeContext();
  }

  public Syntax? Parent { get; private set; }


  public abstract void WriteAsSyntax(ITypeAwareSyntaxWriter writer);

  internal void SetParent(Syntax parent) {
    Parent = parent;
  }
}
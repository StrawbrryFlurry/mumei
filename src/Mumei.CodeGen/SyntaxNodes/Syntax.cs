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

  /// <inheritdoc cref="Clone" />
  public TSyntax Clone<TSyntax>() where TSyntax : Syntax {
    return (TSyntax)Clone();
  }

  /// <summary>
  ///   Clones the current syntax node, creating
  ///   an exact replica without a parent defined
  ///   for it. Additionally creates a clone of all
  ///   child nodes that the syntax might have and
  ///   sets the parent of the cloned nodes to the
  ///   new syntax.
  /// </summary>
  /// <returns></returns>
  public abstract Syntax Clone();

  internal void SetParent(Syntax parent) {
    if (Parent is not null) {
      throw new InvalidOperationException(
        "Syntax node already has a parent defined. Use <Syntax>.Clone to create a copy of the current node");
    }

    Parent = parent;
  }
}
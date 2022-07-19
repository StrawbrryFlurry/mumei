using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class Syntax {
  public readonly string Identifier;
  public readonly Syntax? Parent;
  protected SyntaxTypeContext TypeContext;

  protected Syntax(string identifier) {
    Identifier = identifier;
    TypeContext = new SyntaxTypeContext();
  }

  protected Syntax(string identifier, Syntax? parent) {
    Identifier = identifier;
    Parent = parent;
    TypeContext = parent?.TypeContext ?? new SyntaxTypeContext();
  }

  /// <summary>
  ///   Returns the identifier for the member.
  /// </summary>
  /// <returns></returns>
  public virtual string GetIdentifier() {
    return Identifier;
  }

  public abstract void WriteAsSyntax(ITypeAwareSyntaxWriter writer);
}
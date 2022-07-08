using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class Syntax {
  public readonly string Identifier;
  public readonly Syntax? Parent;
  protected SyntaxTypeContext TypeContext;
  public SyntaxVisibility Visibility { get; set; } = SyntaxVisibility.None;
  public AttributeUsage[] Attributes { get; set; } = Array.Empty<AttributeUsage>();

  public bool HasAttributes => Attributes.Length > 0;

  protected Syntax(string identifier) {
    Identifier = identifier;
    TypeContext = new SyntaxTypeContext();
  }

  protected Syntax(string identifier, Syntax? parent) {
    Identifier = identifier;
    Parent = parent;
    TypeContext = parent?.TypeContext ?? new SyntaxTypeContext();
  }

  protected internal string? GetAttributeSyntax(bool sameLine = false) {
    if (!Attributes.Any()) {
      return null;
    }

    var writer = new AttributeSyntaxWriter(TypeContext);
    writer.WriteAttributes(Attributes, sameLine);

    return writer.ToSyntax();
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
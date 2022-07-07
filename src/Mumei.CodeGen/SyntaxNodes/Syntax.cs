using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class Syntax {
  public readonly string Name;
  public readonly Syntax? Parent;
  protected SyntaxTypeContext TypeContext;
  public SyntaxVisibility Visibility { get; init; } = SyntaxVisibility.None;
  public AttributeUsage[] Attributes { get; init; } = Array.Empty<AttributeUsage>();

  public bool HasAttributes => Attributes.Length > 0;

  protected Syntax(string name) {
    Name = name;
    TypeContext = new SyntaxTypeContext();
  }

  protected Syntax(string name, Syntax? parent) {
    Name = name;
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
    return Name;
  }

  public abstract void WriteAsSyntax(ITypeAwareSyntaxWriter writer);
}
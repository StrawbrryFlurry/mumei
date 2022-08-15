using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   Represents a syntax node that is part of a type
///   declaration, or member.
/// </summary>
public abstract class TypeSyntax : Syntax {
  public TypeSyntax(string identifier) : base(identifier) { }
  public TypeSyntax(string identifier, Syntax? parent) : base(identifier, parent) { }

  public virtual AttributeListSyntax AttributeList { get; } = new(SeparationStrategy.NewLine);

  public SyntaxVisibility Visibility { get; set; } = SyntaxVisibility.None;

  public bool HasAttributes => AttributeList.Count > 0;

  protected internal void WriteAttributes(ITypeAwareSyntaxWriter writer) {
    AttributeList.WriteAsSyntax(writer);
  }

  public Syntax SetVisibility(SyntaxVisibility visibility) {
    Visibility = visibility;
    return this;
  }
}
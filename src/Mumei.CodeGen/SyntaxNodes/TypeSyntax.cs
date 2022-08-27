using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   Represents a syntax node that is part of a type
///   declaration, or member.
/// </summary>
public abstract class TypeSyntax : Syntax {
  public readonly string Identifier;

  public TypeSyntax(string identifier) {
    Identifier = identifier;
  }

  public TypeSyntax(string identifier, Syntax? parent) : base(parent) {
    Identifier = identifier;
  }

  public virtual AttributeListSyntax AttributeList { get; } = new(SeparationStrategy.NewLine);

  public virtual SyntaxVisibility Visibility { get; set; } = SyntaxVisibility.None;

  public bool HasAttributes => AttributeList.Count > 0;

  protected void WriteAttributes(ITypeAwareSyntaxWriter writer) {
    AttributeList.WriteAsSyntax(writer);
  }

  protected void WriteVisibility(ITypeAwareSyntaxWriter writer) {
    writer.Write(Visibility);
  }

  public TypeSyntax SetVisibility(SyntaxVisibility visibility) {
    Visibility = visibility;
    return this;
  }

  /// <summary>
  ///   Writes the attribute list, visibility modifiers and
  ///   identifier to the syntax writer
  ///   [...Attributes]
  ///   [public ...] [identifier]
  /// </summary>
  /// <param name="writer"></param>
  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    if (HasAttributes) {
      WriteAttributes(writer);
      writer.WriteLine();
    }

    WriteVisibility(writer);
    writer.Write(GetIdentifier());
  }

  /// <summary>
  ///   Returns the identifier for the member.
  /// </summary>
  /// <returns></returns>
  public virtual string GetIdentifier() {
    return Identifier;
  }
}
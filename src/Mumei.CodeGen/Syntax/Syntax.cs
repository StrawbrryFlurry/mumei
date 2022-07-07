﻿using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.Syntax;

public abstract class Syntax {
  public readonly AttributeUsage[] Attributes;
  public readonly string Name;
  public readonly SyntaxVisibility Visibility;

  protected internal SyntaxTypeContext TypeContext;

  protected Syntax(SyntaxConfiguration config) {
    Name = config.Name;
    Visibility = config.Visibility;
    Attributes = config.Attributes;
    TypeContext = config.TypeContext;
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
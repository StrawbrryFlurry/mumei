using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   Represents a syntax node that is part of a type
///   declaration or member.
/// </summary>
public abstract class TypeSyntax : Syntax {
  private List<AttributeUsage> _attributes = new();

  public TypeSyntax(string identifier) : base(identifier) { }
  public TypeSyntax(string identifier, Syntax? parent) : base(identifier, parent) { }

  public SyntaxVisibility Visibility { get; set; } = SyntaxVisibility.None;

  public IEnumerable<AttributeUsage> Attributes {
    get => _attributes;
    set => _attributes = value.ToList();
  }

  public bool HasAttributes => _attributes.Count > 0;

  protected internal string? GetAttributeSyntax(bool sameLine = false) {
    if (!Attributes.Any()) return null;

    var writer = new AttributeSyntaxWriter(TypeContext);
    writer.WriteAttributes(Attributes.ToArray(), sameLine);

    return writer.ToSyntax();
  }

  public Syntax SetVisibility(SyntaxVisibility visibility) {
    Visibility = visibility;
    return this;
  }

  public Syntax AddAttribute<TAttribute>(params object[] arguments) where TAttribute : Attribute {
    return AddAttribute(typeof(TAttribute), arguments);
  }

  public Syntax AddAttribute<TAttribute>(
    Dictionary<NamedAttributeParameter, object> namedArguments,
    params object[] arguments
  ) where TAttribute : Attribute {
    return AddAttribute(typeof(TAttribute), namedArguments, arguments);
  }

  public Syntax AddAttribute(Type attributeType, params object[] arguments) {
    _attributes.Add(
      new AttributeUsage(attributeType) {
        Arguments = arguments
      }
    );

    return this;
  }

  public Syntax AddAttribute(
    Type attributeType,
    Dictionary<NamedAttributeParameter, object> namedArguments,
    params object[] arguments
  ) {
    _attributes.Add(
      new AttributeUsage(attributeType) {
        Arguments = arguments,
        NamedArguments = namedArguments
      }
    );

    return this;
  }
}
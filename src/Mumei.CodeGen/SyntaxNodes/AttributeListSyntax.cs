using System.Collections;
using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   Represents a list of attributes on a type syntax.
/// </summary>
public class AttributeListSyntax : Syntax, ICollection<AttributeSyntax> {
  private readonly List<AttributeSyntax> _attributes = new();
  private readonly SeparationStrategy _separationStrategy;

  public AttributeListSyntax() : base("<AttributeList>") {
    _separationStrategy = SeparationStrategy.None;
  }

  public AttributeListSyntax(SeparationStrategy separationStrategy) : base("<AttributeList>") {
    _separationStrategy = separationStrategy;
  }

  public AttributeListSyntax(SeparationStrategy separationStrategy, Syntax? parent) :
    base("<AttributeList>", parent) {
    _separationStrategy = separationStrategy;
  }

  public IEnumerator<AttributeSyntax> GetEnumerator() {
    return _attributes.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }

  public void Add(AttributeSyntax item) {
    _attributes.Add(item);
  }

  public void Clear() {
    _attributes.Clear();
  }

  public bool Contains(AttributeSyntax item) {
    return _attributes.Contains(item);
  }

  public void CopyTo(AttributeSyntax[] array, int arrayIndex) {
    _attributes.CopyTo(array, arrayIndex);
  }

  public bool Remove(AttributeSyntax item) {
    return _attributes.Remove(item);
  }

  public int Count => _attributes.Count;
  public bool IsReadOnly { get; } = false;

  public override void WriteAsSyntax(ITypeAwareSyntaxWriter writer) {
    foreach (var attribute in _attributes) {
      attribute.WriteAsSyntax(writer);
      _separationStrategy.WriteSeparator(writer);
    }
  }

  public AttributeListSyntax AddAttribute<TAttribute>() {
    var attribute = new AttributeSyntax(typeof(TAttribute), this);
    _attributes.Add(attribute);

    return this;
  }

  public AttributeListSyntax AddAttribute<TAttribute>(params object[] positionalArguments) {
    var attribute = new AttributeSyntax(typeof(TAttribute), this) {
      PositionalArguments = positionalArguments
    };
    _attributes.Add(attribute);


    return this;
  }

  public AttributeListSyntax AddAttribute<TAttribute>(
    Dictionary<NamedAttributeParameter, object> namedArguments,
    params object[] positionalArguments
  ) {
    var attribute = new AttributeSyntax(typeof(TAttribute), this) {
      PositionalArguments = positionalArguments,
      NamedArguments = namedArguments
    };
    _attributes.Add(attribute);

    return this;
  }
}
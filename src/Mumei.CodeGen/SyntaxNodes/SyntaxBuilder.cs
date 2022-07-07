using Mumei.CodeGen.SyntaxWriters;

namespace Mumei.CodeGen.SyntaxNodes;

public abstract class SyntaxBuilder<TSyntax> where TSyntax : Syntax {
  protected readonly List<AttributeUsage> Attributes = new();

  protected readonly string Name;
  protected readonly Syntax? Parent;

  protected SyntaxVisibility Visibility = SyntaxVisibility.None;

  protected SyntaxBuilder(string name, Syntax? parent) {
    Name = name;
    Parent = parent;
  }

  /// <summary>
  ///   Returns the built syntax node.
  /// </summary>
  /// <returns></returns>
  public abstract TSyntax Build();

  /// <summary>
  ///   Creates an instance of the syntax node
  ///   adding all the builder's base properties to it.
  /// </summary>
  /// <returns></returns>
  protected virtual TSyntax MakeSyntaxInstance() {
    var instance = (Syntax) Activator.CreateInstance(typeof(TSyntax), Name, Parent)!;

    instance.Attributes = Attributes.ToArray();
    instance.Visibility = Visibility;

    return (TSyntax) instance;
  }

  public SyntaxBuilder<TSyntax> SetVisibility(SyntaxVisibility visibility) {
    Visibility = visibility;
    return this;
  }

  public SyntaxBuilder<TSyntax> AddAttribute<TAttribute>(params object[] arguments) where TAttribute : Attribute {
    return AddAttribute(typeof(TAttribute), arguments);
  }

  public SyntaxBuilder<TSyntax> AddAttribute<TAttribute>(
    Dictionary<NamedAttributeParameter, object> namedArguments,
    params object[] arguments
  ) where TAttribute : Attribute {
    return AddAttribute(typeof(TAttribute), namedArguments, arguments);
  }

  public SyntaxBuilder<TSyntax> AddAttribute(Type attributeType, params object[] arguments) {
    Attributes.Add(
      new AttributeUsage(attributeType) {
        Arguments = arguments
      }
    );

    return this;
  }

  public SyntaxBuilder<TSyntax> AddAttribute(
    Type attributeType,
    Dictionary<NamedAttributeParameter, object> namedArguments,
    params object[] arguments
  ) {
    Attributes.Add(
      new AttributeUsage(attributeType) {
        Arguments = arguments,
        NamedArguments = namedArguments
      }
    );

    return this;
  }
}
namespace Mumei.CodeGen.SyntaxNodes;

public abstract class MemberSyntax : TypeSyntax {
  /// <summary>
  ///   The Syntax in which this member is declared.
  /// </summary>
  public new readonly Syntax Parent;

  public MemberSyntax(Type type, string identifier, Syntax parent) : base(identifier, parent) {
    Parent = parent;
    Type = type;
  }

  /// <summary>
  ///   The type of the member or the
  ///   return type if the member is a method.
  /// </summary>
  public Type Type { get; init; }

  /// <summary>
  ///   Priority of the member in the order of declaration.
  ///   Members with a lower priority are declared before members with a higher priority.
  ///   Members of different types with the same priority are ordered alphabetically.
  ///   - Field: 0
  ///   - Property: 1
  ///   - Constructor: 5
  ///   - Method: 10
  /// </summary>
  public abstract int Priority { get; }
}
namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   Represents a variable that can hold a value.
///   Types that implement this
///   interface are also automatically transformed
///   in expressions to use their <see cref="Identifier" />, when
///   the <see cref="Value" /> is accessed.
/// </summary>
/// <typeparam name="T">The type of the value being held</typeparam>
public interface IValueHolderSyntax<T> {
  public string Identifier { get; }
  public T Value { get; set; }
}

/// <summary>
///   Same as <see cref="IValueHolderSyntax{T}" />, but declares the value
///   being present on a containing type which adds a `this.` prefix
///   to the identifier making it an explicit member access.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMemberValueHolderSyntax<T> : IValueHolderSyntax<T> {
  // TODO: Add impl.
}
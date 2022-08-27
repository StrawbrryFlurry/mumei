namespace Mumei.CodeGen.SyntaxNodes;

/// <summary>
///   Represents a variable or field that
///   can hold a value. Types that implement this
///   interface are also automatically transformed
///   in expressions to use their <see cref="Identifier" />, when
///   the <see cref="Value" /> is accessed.
/// </summary>
/// <typeparam name="T">The type of the value being held</typeparam>
public interface IValueHolderSyntax<T> {
  public string Identifier { get; }
  public T? Value { get; set; }
}
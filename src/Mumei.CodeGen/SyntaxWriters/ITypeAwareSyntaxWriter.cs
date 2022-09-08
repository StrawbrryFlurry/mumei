namespace Mumei.CodeGen.SyntaxWriters;

public interface ITypeAwareSyntaxWriter : ISyntaxWriter {
  public SyntaxTypeContext TypeContext { get; }

  /// <summary>
  ///   Converts any value to it's expression form.
  ///   String foo => "foo"
  ///   Type string => typeof(string)
  ///   int 1 => 1
  ///   bool true => true
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  public void WriteValueAsExpressionSyntax(object value);

  /// <summary>
  ///   Returns the name of a type as it is used in the syntax.
  ///   typeof(IEnumerable{string}) => "IEnumerable{string}"
  /// </summary>
  /// <param name="type"></param>
  /// <returns></returns>
  public void WriteTypeName(Type type);
}
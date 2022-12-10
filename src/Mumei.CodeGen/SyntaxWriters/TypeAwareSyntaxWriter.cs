using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxWriters;

public class TypeAwareSyntaxWriter : SyntaxWriter, ITypeAwareSyntaxWriter {
  public TypeAwareSyntaxWriter(SyntaxTypeContext? ctx = null) {
    TypeContext = ctx ?? new SyntaxTypeContext();
  }

  public SyntaxTypeContext TypeContext { get; }

  public void WriteValueAsExpressionSyntax(object value) {
    Write(GetValueAsExpressionSyntax(value));
  }

  public void WriteTypeName(Type type) {
    Write(GetTypeName(type));
  }

  protected internal string GetValueAsExpressionSyntax(object? value) {
    return value switch {
      bool b => b ? "true" : "false",
      Enum e => $"{e.GetType().Name}.{e}",
      Type type => $"typeof({GetTypeName(type)})",
      _ => GetUnknownExpressionValueAsString(value)
    };
  }

  protected internal string GetTypeName(Type type) {
    if (type.IsGenericType) {
      return GetGenericTypeAsString(type);
    }

    return GetNonGenericTypeAsString(type);
  }

  private string GetGenericTypeAsString(Type type) {
    var genericName = GetNonGenericTypeAsString(type);
    var typeName = Regex.Replace(genericName, "`.*", "");
    var genericArguments = type.GetGenericArguments();

    var genericArgumentString = genericArguments.Select(GetTypeName).JoinBy(", ");
    return $"{typeName}<{genericArgumentString}>";
  }

  private string GetNonGenericTypeAsString(Type type) {
    TypeContext.IncludeTypeNamespace(type);
    return type.Name;
  }

  private string GetUnknownExpressionValueAsString(object? value) {
    return Expression.Constant(value).ToString();
  }
}
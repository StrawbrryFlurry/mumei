using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxWriters;

public class TypeAwareSyntaxWriter : SyntaxWriter {
  private readonly SyntaxTypeContext _ctx;

  public TypeAwareSyntaxWriter(SyntaxTypeContext ctx) {
    _ctx = ctx;
  }

  public void IncludeTypeNamespace(Type type) {
    var ns = type.Namespace;

    if (ns is not null) {
      _ctx.UseNamespace(ns);
    }

    if (!type.IsGenericType) {
      return;
    }

    var args = type.GetGenericArguments();

    foreach (var arg in args) {
      IncludeTypeNamespace(arg);
    }
  }

  protected internal string ConvertExpressionValueToSyntax(object value) {
    return value switch {
      bool b => b ? "true" : "false",
      Enum e => $"{e.GetType().Name}.{e}",
      Type {IsGenericType: true} type => $"typeof({GetGenericTypeofExpressionAsString(type)})",
      Type type => $"typeof({GetTypeofExpressionAsString(type)})",
      _ => GetUnknownExpressionValueAsString(value)
    };
  }

  private string GetGenericTypeofExpressionAsString(Type type) {
    var genericName = GetTypeofExpressionAsString(type);
    var typeName = Regex.Replace(genericName, "`.*", "");
    var genericArguments = type.GetGenericArguments();

    var genericArgumentString = genericArguments.Select(GetTypeofExpressionAsString).JoinBy(", ");
    return $"{typeName}<{genericArgumentString}>";
  }

  private string GetTypeofExpressionAsString(Type type) {
    IncludeTypeNamespace(type);
    return type.Name;
  }

  private string GetUnknownExpressionValueAsString(object value) {
    return Expression.Constant(value).ToString();
  }
}
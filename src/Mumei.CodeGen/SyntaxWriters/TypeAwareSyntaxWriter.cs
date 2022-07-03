using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxWriters;

public class TypeAwareSyntaxWriter : SyntaxWriter {
  private readonly WriterTypeContext _ctx;

  public TypeAwareSyntaxWriter(WriterTypeContext ctx) {
    _ctx = ctx;
  }

  protected internal void IncludeTypeNamespace(Type type) {
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

  protected internal string ConvertValueToStringRepresentation(object value) {
    return value switch {
      bool b => b ? "true" : "false",
      Enum e => $"{e.GetType().Name}.{e}",
      Type {IsGenericType: true} type => $"typeof({GetGenericTypeValueAsString(type)})",
      Type type => $"typeof({GetTypeValueAsString(type)})",
      _ => GetUnknownValueAsString(value)
    };
  }

  private string GetGenericTypeValueAsString(Type type) {
    var genericName = GetTypeValueAsString(type);
    var typeName = Regex.Replace(genericName, "`.*", "");
    var genericArguments = type.GetGenericArguments();

    var genericArgumentString = genericArguments.Select(GetTypeValueAsString).JoinBy(", ");
    return $"{typeName}<{genericArgumentString}>";
  }

  private string GetTypeValueAsString(Type type) {
    IncludeTypeNamespace(type);
    return type.Name;
  }

  private string GetUnknownValueAsString(object value) {
    return Expression.Constant(value).ToString();
  }
}
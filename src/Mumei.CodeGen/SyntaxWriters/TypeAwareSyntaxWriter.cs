using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Mumei.CodeGen.Extensions;

namespace Mumei.CodeGen.SyntaxWriters;

public class TypeAwareSyntaxWriter : SyntaxWriter, ITypeAwareSyntaxWriter {
  private readonly SyntaxTypeContext? _ctx;

  public TypeAwareSyntaxWriter(SyntaxTypeContext? ctx) {
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

  public void WriteValueAsExpressionSyntax(object value) {
    Write(GetValueAsExpressionSyntax(value));
  }

  public void WriteTypeName(Type type) {
    Write(GetTypeName(type));
  }

  protected internal string GetValueAsExpressionSyntax(object value) {
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
    IncludeTypeNamespace(type);
    return type.Name;
  }

  private string GetUnknownExpressionValueAsString(object value) {
    return Expression.Constant(value).ToString();
  }
}
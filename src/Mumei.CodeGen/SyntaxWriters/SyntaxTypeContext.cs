﻿namespace Mumei.CodeGen.SyntaxWriters;

/// <summary>
///   Contains all namespaces used by a container type (class, struct, enum, etc.)
/// </summary>
public class SyntaxTypeContext {
  private readonly HashSet<string> _usedNamespaces = new();
  internal IEnumerable<string> UsedNamespaces => _usedNamespaces;

  public void UseNamespace(string ns) {
    _usedNamespaces.Add(ns);
  }

  public void IncludeTypeNamespace(Type type) {
    var ns = type.Namespace;

    if (ns is not null) {
      UseNamespace(ns);
    }

    if (!type.IsGenericType) {
      return;
    }

    var args = type.GetGenericArguments();

    foreach (var arg in args) {
      IncludeTypeNamespace(arg);
    }
  }
}
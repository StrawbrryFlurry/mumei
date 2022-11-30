using System.Text;
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn;

public static class SymbolExtensions {
  public static string GetFullName(this INamespaceOrTypeSymbol typeSymbol) {
    if (typeSymbol.ContainingNamespace.IsGlobalNamespace) {
      return typeSymbol.Name;
    }

    var namespaces = new List<string>();
    var ns = typeSymbol.ContainingNamespace;
    while (!ns.IsGlobalNamespace) {
      namespaces.Add(ns.Name);
      ns = ns.ContainingNamespace;
    }

    var fqnBuilder = new StringBuilder();
    namespaces.Reverse();

    foreach (var @namespace in namespaces) {
      fqnBuilder.Append(@namespace);
      fqnBuilder.Append('.');
    }

    fqnBuilder.Append(typeSymbol.Name);

    return fqnBuilder.ToString();
  }
}
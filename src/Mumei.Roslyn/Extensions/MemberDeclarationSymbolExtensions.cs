﻿// ReSharper disable all ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.Roslyn.Extensions;

public static class MemberDeclarationSymbolExtensions {
  public static bool TryGetAttribute(
    this MemberDeclarationSyntax memberDeclarationSyntax,
    SemanticModel semanticModel,
    string attributeName,
    out IMethodSymbol? attributeSymbol
  ) {
    foreach (var attributeListSyntax in memberDeclarationSyntax.AttributeLists) {
      foreach (var attributeSyntax in attributeListSyntax.Attributes) {
        var attributeInfo = semanticModel.GetSymbolInfo(attributeSyntax);
        var attributeSymbolCandidate = attributeInfo.Symbol
                                       ?? attributeInfo.CandidateSymbols
                                         .FirstOrDefault(s => s is IMethodSymbol);
        if (attributeSymbolCandidate is not IMethodSymbol attrSymbol) {
          attributeSymbol = null;
          continue;
        }

        var memberAttributeName = attrSymbol.ContainingType.ToDisplayString();
        if (attributeName != memberAttributeName) {
          continue;
        }

        attributeSymbol = attrSymbol;
        return true;
      }
    }

    attributeSymbol = null;
    return false;
  }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.Roslyn.Extensions;

public static class MemberDeclarationSymbolExtensions {
  public static bool HasAttribute(
    this MemberDeclarationSyntax memberDeclarationSyntax,
    SemanticModel semanticModel,
    string attributeName,
    out IMethodSymbol? attributeSymbol
  ) {
    // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
    foreach (var attributeListSyntax in memberDeclarationSyntax.AttributeLists) {
      foreach (var attributeSyntax in attributeListSyntax.Attributes) {
        if (semanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attrSymbol) {
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
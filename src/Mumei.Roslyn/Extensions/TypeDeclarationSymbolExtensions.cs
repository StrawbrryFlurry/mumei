using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mumei.Roslyn.Extensions;

public static class TypeDeclarationSymbolExtensions {
  public static bool TryGetTypeSymbol(
    this Compilation compilation,
    TypeDeclarationSyntax typeDeclarationSyntax,
    out INamedTypeSymbol typeSymbol
  ) {
    var semanticModel = compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree);
    if (semanticModel.GetDeclaredSymbol(typeDeclarationSyntax) is not INamedTypeSymbol foundTypeSymbol) {
      typeSymbol = null!;
      return false;
    }

    typeSymbol = foundTypeSymbol;
    return true;
  }
}
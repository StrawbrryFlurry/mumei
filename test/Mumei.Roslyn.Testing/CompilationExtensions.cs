using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing;

public static class CompilationExtensions {
  public static TSymbol GetSymbolByName<TSymbol>(this Compilation compilation, string name) where TSymbol : ISymbol {
    var x = compilation.GetSymbolsWithName(name, SymbolFilter.All);
    return x.OfType<TSymbol>().First();
  }

  public static INamedTypeSymbol GetTypeSymbol(this Compilation compilation, string typeName) {
    return compilation.GetTypeByMetadataName(typeName)!; // We assume consumers know their type names
  }

  public static TMember GetTypeMemberSymbol<TMember>(this Compilation compilation, string typeName, string memberName)
    where TMember : ISymbol {
    var typeSymbol = GetTypeSymbol(compilation, typeName);
    return typeSymbol.GetMembers().OfType<TMember>().First(x => x.Name == memberName);
  }
}
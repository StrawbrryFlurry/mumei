using Microsoft.CodeAnalysis;
using Mumei.Roslyn.Testing.Template;

namespace Mumei.Roslyn.Testing;

public static class CompilationExtensions {
    public static TSymbol GetSymbolByName<TSymbol>(this Compilation compilation, string name)
        where TSymbol : ISymbol {
        var s = compilation.GetSymbolsWithName(name, SymbolFilter.All);
        return s.OfType<TSymbol>().First();
    }

    public static INamedTypeSymbol GetTypeSymbol(this Compilation compilation, string typeName) {
        return compilation.GetTypeByMetadataName(typeName)!; // We assume consumers know their type names
    }

    public static INamedTypeSymbol GetTypeSymbol(this Compilation compilation, CompilationType type) {
        return compilation.GetTypeByMetadataName(type.FullName)!;
    }

    public static TMember GetTypeMemberSymbol<TMember>(this Compilation compilation,
        string typeName, string memberName)
        where TMember : ISymbol {
        var typeSymbol = GetTypeSymbol(compilation, typeName);
        return typeSymbol.GetMembers().OfType<TMember>().First(x => x.Name == memberName);
    }
}
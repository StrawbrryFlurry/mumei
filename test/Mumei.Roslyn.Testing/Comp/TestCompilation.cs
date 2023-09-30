using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing.Comp;

public static class TestCompilation {
  public static TSymbol GetSymbolByNameFromSource<TSymbol>(
    TypeSource source
  ) where TSymbol : ISymbol {
    return TestCompilationBuilder.CreateFromSources(source).Build().GetSymbolByName<TSymbol>(source.Name);
  }

  public static TSymbol GetSymbolByNameFromSource<TSymbol>(
    string source,
    Action<SourceFileBuilder>? configure = null,
    [CallerArgumentExpression(nameof(source))]
    string symbolName = ""
  ) where TSymbol : ISymbol {
    return CompileFromSource(source, configure).GetSymbolByName<TSymbol>(symbolName);
  }

  public static Compilation CompileFromSource(string source, Action<SourceFileBuilder>? configure = null) {
    return new TestCompilationBuilder().AddSource(source, configure).Build();
  }

  public static Compilation FromSource(params TypeSource[] sources) {
    return TestCompilationBuilder.CreateFromSources(sources).Build();
  }

  public static ITypeSymbol CompileTypeSymbol(TypeSource source) {
    return FromSource(source).GetTypeSymbol(source.MetadataName);
  }

  public static ITypeSymbol CompileTypeSymbol(TypeSource source, out Compilation compilation) {
    compilation = FromSource(source);
    return compilation.GetTypeSymbol(source.MetadataName);
  }

  public static T CompileToSymbol<T>(TypeSource source) {
    return (T)FromSource(source).GetTypeSymbol(source.MetadataName);
  }
}
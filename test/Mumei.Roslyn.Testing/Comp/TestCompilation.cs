using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Testing.Comp;

public static class TestCompilation {
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
}
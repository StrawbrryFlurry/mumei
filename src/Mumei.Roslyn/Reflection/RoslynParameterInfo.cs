using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public readonly struct RoslynParameterInfo {
  private readonly IParameterSymbol _symbol;

  public RoslynType Type => new(_symbol.Type);

  public RoslynParameterInfo(IParameterSymbol symbol) {
    _symbol = symbol;
  }
}
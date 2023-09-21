using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public struct RoslynMethodInfo {
  private readonly IMethodSymbol _symbol;

  public RoslynMethodInfo(IMethodSymbol symbol) {
    _symbol = symbol;
  }
}
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public readonly struct RoslynMethodInfo {
  private readonly IMethodSymbol _symbol;

  public string Name => _symbol.Name;

  public RoslynType ReturnType => new(_symbol.ReturnType);

  public RoslynMethodInfo(IMethodSymbol symbol) {
    _symbol = symbol;
  }

  public TemporarySpan<RoslynAttribute> GetAttributesTemp() {
    return RoslynAttributeCollector.GetAttributesTemp(_symbol);
  }

  public ImmutableArray<RoslynType> GetParameters() {
    return GetParametersBag().ToImmutableArrayAndFree();
  }

  private ArrayBuilder<RoslynType> GetParametersBag() {
    var parameters = _symbol.Parameters;
    var builder = new ArrayBuilder<RoslynType>(parameters.Length);
    for (var i = 0; i < parameters.Length; i++) {
      builder.Add(new RoslynType(parameters[i].Type));
    }

    return builder;
  }
}
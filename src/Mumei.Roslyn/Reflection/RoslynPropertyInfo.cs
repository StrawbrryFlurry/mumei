using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public readonly struct RoslynPropertyInfo {
  private readonly IPropertySymbol _property;

  public RoslynType Type => new(_property.Type);

  public string Name => _property.Name;

  public RoslynPropertyInfo(IPropertySymbol property) {
    _property = property;
  }

  public TemporarySpan<RoslynAttribute> GetAttributesTemp() {
    return RoslynAttributeCollector.GetAttributesTemp(_property);
  }

  private object GetTypeArgumentsBag() {
    throw new NotImplementedException();
  }
}
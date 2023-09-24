using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public readonly ref struct RoslynAttributeCollector {
  public static TemporarySpan<RoslynAttribute> GetAttributesTemp(ISymbol symbol) {
    var symbolAttributes = symbol.GetAttributes();
    return GetAttributesBag(symbolAttributes).AsRefForfeitOwnership();
  }

  public static ReadOnlySpan<RoslynAttribute> GetAttributes(ISymbol symbol) {
    var symbolAttributes = symbol.GetAttributes();
    return GetAttributesBag(symbolAttributes).ToReadOnlySpanAndFree();
  }

  private static ArrayBuilder<RoslynAttribute> GetAttributesBag(ImmutableArray<AttributeData> attributes) {
    if (attributes.Length == 0) {
      return ArrayBuilder<RoslynAttribute>.Empty;
    }

    var attributesBag = new ArrayBuilder<RoslynAttribute>(attributes.Length);
    var a = attributes.AsSpan();
    for (var i = 0; i < a.Length; i++) {
      var attribute = a[i];
      attributesBag.Add(new RoslynAttribute(attribute));
    }

    return attributesBag;
  }
}
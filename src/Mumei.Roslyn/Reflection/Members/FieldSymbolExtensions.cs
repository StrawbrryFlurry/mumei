using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection;

public static class FieldSymbolExtensions {
  internal static IFieldInfoFactory ToFieldInfoFactory(this IFieldSymbol fieldSymbol) {
    return new FieldInfoFactory(fieldSymbol);
  }

  public static FieldInfo ToPropertyInfo(this IFieldSymbol fieldSymbol, Type declaringType) {
    return FieldInfoFactory.CreateFieldInfo(fieldSymbol, declaringType);
  }

  private sealed class FieldInfoFactory : IFieldInfoFactory {
    private readonly IFieldSymbol _symbol;

    public FieldInfoFactory(IFieldSymbol symbol) {
      _symbol = symbol;
    }

    public FieldInfo CreateFieldInfo(Type declaringType) {
      return CreateFieldInfo(_symbol, declaringType);
    }

    public static FieldInfo CreateFieldInfo(IFieldSymbol symbol, Type declaringType) {
      return ReflectionFieldInfo.Create(
        symbol.Name,
        declaringType,
        GetFieldAttributes(symbol),
        symbol.GetCustomAttributeData(),
        declaringType
      );
    }

    private static FieldAttributes GetFieldAttributes(IFieldSymbol symbol) {
      var attributes = default(FieldAttributes);

      if (symbol.IsStatic) {
        attributes |= FieldAttributes.Static;
      }

      if (symbol.IsReadOnly) {
        attributes |= FieldAttributes.InitOnly;
      }

      switch (symbol.DeclaredAccessibility) {
        case Accessibility.Public:
          attributes |= FieldAttributes.Public;
          break;
        case Accessibility.Private:
          attributes |= FieldAttributes.Private;
          break;
        case Accessibility.Protected:
          attributes |= FieldAttributes.Family;
          break;
      }

      return attributes;
    }
  }
}

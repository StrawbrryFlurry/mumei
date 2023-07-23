using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common;

namespace Mumei.Roslyn.Reflection;

public static class FieldSymbolExtensions {
  internal static IFieldInfoFactory ToFieldInfoFactory(this IFieldSymbol fieldSymbol) {
    return new FieldInfoFactory(fieldSymbol);
  }

  public static FieldInfo ToFieldInfo(this IFieldSymbol fieldSymbol, Type declaringType) {
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
        symbol.Type.ToType(),
        GetFieldAttributes(symbol),
        symbol.ToCustomAttributeCollection(),
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

      if (symbol.IsConst) {
        attributes |= FieldAttributes.Literal;
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
        case Accessibility.Internal:
          attributes |= FieldAttributes.Assembly;
          break;
        case Accessibility.ProtectedOrInternal:
          attributes |= FieldAttributes.FamORAssem;
          break;
      }

      return attributes;
    }
  }
}
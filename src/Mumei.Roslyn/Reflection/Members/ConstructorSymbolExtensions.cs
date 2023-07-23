using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common;

namespace Mumei.Roslyn.Reflection;

public static class ConstructorSymbolExtensions {
  internal static IConstructorInfoFactory ToConstructorInfoFactory(this IMethodSymbol symbol) {
    return new ConstructorInfoFactory(symbol);
  }

  internal static ConstructorInfo ToConstructorInfo(this IMethodSymbol symbol, Type declaringType) {
    return ConstructorInfoFactory.CreateConstructorInfo(symbol, declaringType);
  }

  private sealed class ConstructorInfoFactory : IConstructorInfoFactory {
    private readonly IMethodSymbol _symbol;

    public ConstructorInfoFactory(IMethodSymbol symbol) {
      _symbol = symbol;
    }

    public ConstructorInfo CreateConstructorInfo(Type declaringType) {
      return CreateConstructorInfo(_symbol, declaringType);
    }

    public static ConstructorInfo CreateConstructorInfo(IMethodSymbol symbol, Type declaringType) {
      return ReflectionConstructorInfo.Create(
        symbol.Name,
        GetMethodAttributes(symbol),
        declaringType
      );
    }

    private static MethodAttributes GetMethodAttributes(IMethodSymbol symbol) {
      MethodAttributes attributes = default;

      if (symbol.IsAbstract) {
        attributes |= MethodAttributes.Abstract | MethodAttributes.Virtual;
      }

      if (symbol.IsStatic) {
        attributes |= MethodAttributes.Static;
      }

      if (symbol.IsVirtual || symbol.IsOverride) {
        attributes |= MethodAttributes.Virtual;
      }

      switch (symbol.DeclaredAccessibility) {
        case Accessibility.Public:
          attributes |= MethodAttributes.Public;
          break;
        case Accessibility.Private:
          attributes |= MethodAttributes.Private;
          break;
        case Accessibility.Internal:
          attributes |= MethodAttributes.Assembly;
          break;
      }

      if (symbol.MethodKind != MethodKind.Ordinary) {
        attributes |= MethodAttributes.SpecialName;
      }

      return attributes;
    }
  }
}
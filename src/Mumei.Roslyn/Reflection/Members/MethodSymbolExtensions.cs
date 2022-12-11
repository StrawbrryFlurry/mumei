using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection;

public static class MethodSymbolExtensions {
  internal static IMethodInfoFactory ToMethodInfoFactory(this IMethodSymbol symbol) {
    return new MethodInfoFactory(symbol);
  }

  internal static MethodInfo ToMethodInfo(this IMethodSymbol symbol, Type declaringType) {
    return MethodInfoFactory.CreateMethodInfo(symbol, declaringType);
  }

  private sealed class MethodInfoFactory : IMethodInfoFactory {
    private readonly IMethodSymbol _symbol;

    public MethodInfoFactory(IMethodSymbol symbol) {
      _symbol = symbol;
    }

    public MethodInfo CreateMethodInfo(Type declaringType) {
      return CreateMethodInfo(_symbol, declaringType);
    }

    public static MethodInfo CreateMethodInfo(IMethodSymbol symbol, Type declaringType) {
      return ReflectionMethodInfo.Create(
        symbol.Name,
        symbol.ReturnType.ToType(),
        symbol.Parameters.Select(p => p.ToParameterInfo(declaringType)).ToArray(),
        symbol.TypeArguments.Select(t => t.ToType()).ToArray(),
        GetMethodAttributes(symbol),
        symbol.MethodImplementationFlags,
        symbol.GetAttributes().Select(x => x.ToCustomAttributeData()).ToArray(),
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
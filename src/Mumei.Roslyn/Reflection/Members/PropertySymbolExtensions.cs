using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection;

public static class PropertySymbolExtensions {
  internal static IPropertyInfoFactory ToPropertyInfoFactory(this IPropertySymbol propertySymbol) {
    return new PropertyInfoFactory(propertySymbol);
  }

  public static PropertyInfo ToPropertyInfo(this IPropertySymbol propertySymbol, Type declaringType) {
    return PropertyInfoFactory.CreatePropertyInfo(propertySymbol, declaringType);
  }

  private sealed class PropertyInfoFactory : IPropertyInfoFactory {
    private readonly IPropertySymbol _symbol;

    public PropertyInfoFactory(IPropertySymbol symbol) {
      _symbol = symbol;
    }

    public PropertyInfo CreatePropertyInfo(Type declaringType) {
      return CreatePropertyInfo(_symbol, declaringType);
    }

    public static PropertyInfo CreatePropertyInfo(IPropertySymbol symbol, Type declaringType) {
      return ReflectionPropertyInfo.Create(
        symbol.Name,
        symbol.Type.ToType(),
        symbol.GetMethod!.ToMethodInfo(declaringType), // Write-only properties should not be used
        symbol.SetMethod?.ToMethodInfo(declaringType),
        GetPropertyAttributes(symbol),
        symbol.IsIndexer,
        symbol.Parameters.Select(p => p.ToParameterInfo(declaringType)).ToArray(),
        declaringType
      );
    }

    private static PropertyAttributes GetPropertyAttributes(IPropertySymbol symbol) {
      return PropertyAttributes.None;
    }
  }
}
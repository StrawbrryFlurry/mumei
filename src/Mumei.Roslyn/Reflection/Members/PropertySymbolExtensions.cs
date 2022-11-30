using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection.Members;

namespace Mumei.Roslyn.Reflection.Members; 

public static class PropertySymbolExtensions {
  public static PropertyInfoSpec ToPropertyInfoSpec(this IPropertySymbol propertySymbol) {
    return new PropertyInfoSpec {
      Name = propertySymbol.Name,
      PropertyType = propertySymbol.Type.ToType(),
      CanRead = propertySymbol.GetMethod != null,
      CanWrite = propertySymbol.SetMethod != null,
      GetMethod = propertySymbol.GetMethod!.ToMethodInfoSpec(), // Write-only properties should not be used
      SetMethod = propertySymbol.SetMethod?.ToMethodInfoSpec(),
      IsIndexer = propertySymbol.IsIndexer,
      IndexParameters = propertySymbol.Parameters.Select(p => p.ToParameterInfo()).ToArray()
    };
  }
}
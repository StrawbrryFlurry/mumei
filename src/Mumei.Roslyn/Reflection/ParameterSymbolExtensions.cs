using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection; 

public static class ParameterSymbolExtensions {
  public static ParameterInfo ToParameterInfo(this IParameterSymbol symbol) {
    var customAttributes = symbol.GetAttributes().Select(x => x.ToAttributeDeclaration()).ToList();
    
    return new ReflectionParameterInfo(
      symbol.Name,
      symbol.Type.ToType(),
      customAttributes,
      symbol.Ordinal,
      symbol.HasExplicitDefaultValue,
      symbol.ExplicitDefaultValue
      );
  }
}
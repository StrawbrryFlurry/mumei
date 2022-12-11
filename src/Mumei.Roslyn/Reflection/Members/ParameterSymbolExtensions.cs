using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection;

public static class ParameterSymbolExtensions {
  internal static IParameterInfoFactory ToParameterInfoFactory(this IParameterSymbol symbol) {
    return new ParameterInfoFactory(symbol);
  }

  internal static ParameterInfo ToParameterInfo(this IParameterSymbol symbol, Type declaringType) {
    return ParameterInfoFactory.CreateParameterInfo(symbol, declaringType);
  }

  private sealed class ParameterInfoFactory : IParameterInfoFactory {
    private readonly IParameterSymbol _symbol;

    public ParameterInfoFactory(IParameterSymbol symbol) {
      _symbol = symbol;
    }

    public ParameterInfo CreateParameterInfo(Type declaringType) {
      return CreateParameterInfo(_symbol, declaringType);
    }

    public static ParameterInfo CreateParameterInfo(IParameterSymbol symbol, Type declaringType) {
      var declaringMemberSymbol = symbol.ContainingSymbol;
      var declaringMemberFactory = new SymbolMemberInfoFactory(declaringMemberSymbol);

      return ReflectionParameterInfo.Create(
        symbol.Name,
        declaringType,
        declaringMemberFactory,
        symbol.Type.ToType(),
        symbol.GetCustomAttributeData(),
        symbol.Ordinal,
        symbol.HasExplicitDefaultValue,
        symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue : null
      );
    }
  }
}
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;
using Mumei.Roslyn.Reflection.Members;

namespace Mumei.Roslyn.Reflection; 

public static class TypeSymbolExtensions {
  public static Type ToType(this ITypeSymbol symbol) {
    var fullName = symbol.GetFullName();
    var existingType = Type.GetType(fullName);

    if (existingType is not null) {
      return existingType;
    }

    var module = symbol.ContainingModule.ToModule();
    var members = symbol.GetMembers();
    var mumeiType = ReflectionType.Create(
      symbol.Name,
      symbol.ContainingNamespace.ToDisplayString(),
      symbol.BaseType is null ? null : ToType(symbol.BaseType),
      symbol.Interfaces.Select(ToType).ToArray(),
      symbol.GetTypeMembers().Select(ToType).ToArray(),
      members.OfType<IMethodSymbol>().Select(MethodSymbolExtensions.ToMethodInfoSpec).ToArray(),
      members.OfType<IFieldSymbol>().Select(FieldSymbolExtensions.ToFieldInfoSpec).ToArray(),
      members.OfType<IPropertySymbol>().Select(PropertySymbolExtensions.ToPropertyInfoSpec).ToArray(),
      module
    );

    return mumeiType;
  }
}
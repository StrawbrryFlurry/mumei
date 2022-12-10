using System.Collections.Immutable;
using System.Reflection;
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
    var typeAttributes = GetTypeAttributes(symbol);
    
    var isGenericType = symbol is INamedTypeSymbol { IsGenericType: true };
    
    var mumeiType = ReflectionType.Create(
      symbol.Name,
      symbol.ContainingNamespace.ToDisplayString(),
      symbol.BaseType is null ? null : ToType(symbol.BaseType),
      symbol.Interfaces.Select(ToType).ToArray(),
      symbol.GetGenericArguments(),
      isGenericType,
      typeAttributes,
      members.OfType<IMethodSymbol>().Select(MethodSymbolExtensions.ToMethodInfoSpec).ToArray(),
      members.OfType<IFieldSymbol>().Select(FieldSymbolExtensions.ToFieldInfoSpec).ToArray(),
      members.OfType<IPropertySymbol>().Select(PropertySymbolExtensions.ToPropertyInfoSpec).ToArray(),
      module
      );

    return mumeiType;
  }

  public static Type[] GetGenericArguments(this ITypeSymbol symbol) {
    if (symbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol) {
      return namedTypeSymbol.TypeArguments.Select(ToType).ToArray();
    }

    return Type.EmptyTypes;
  }

  private static TypeAttributes GetTypeAttributes(ITypeSymbol symbol) {
      var attributes = default(TypeAttributes);

      if (symbol.IsAbstract) {
        attributes |= TypeAttributes.Abstract;
      }

      if (symbol.TypeKind == TypeKind.Interface) {
        attributes |= TypeAttributes.Interface;
      }

      if (symbol.IsSealed) {
        attributes |= TypeAttributes.Sealed;
      }

      var isNested = symbol.ContainingType != null;

      switch (symbol.DeclaredAccessibility) {
        case Accessibility.NotApplicable:
        case Accessibility.Private:
          attributes |= isNested ? TypeAttributes.NestedPrivate : TypeAttributes.NotPublic;
          break;
        case Accessibility.ProtectedAndInternal: 
          attributes |= isNested ? TypeAttributes.NestedFamANDAssem : TypeAttributes.NotPublic;
          break;
        case Accessibility.Protected:
          attributes |= isNested ? TypeAttributes.NestedFamily : TypeAttributes.NotPublic;
          break;
        case Accessibility.Internal:
          attributes |= isNested ? TypeAttributes.NestedAssembly : TypeAttributes.NotPublic;
          break;
        case Accessibility.ProtectedOrInternal:
          attributes |= isNested ? TypeAttributes.NestedFamORAssem : TypeAttributes.NotPublic;
          break;
        case Accessibility.Public:
          attributes |= isNested ? TypeAttributes.NestedPublic : TypeAttributes.Public;
          break;
        default:
          break;
      }

      return attributes;
  }
}

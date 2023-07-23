using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common;

namespace Mumei.Roslyn.Reflection;

public static class TypeSymbolExtensions {
  private static readonly Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();

  public static Type ToType(this ITypeSymbol symbol) {
    var fullName = symbol.GetFullName();
    var existingType = GetExistingRuntimeTypeInAssemblies(fullName);

    if (existingType is not null) {
      return existingType;
    }

    var module = symbol.ContainingModule.ToModule();
    var members = symbol.GetMembers();
    var typeAttributes = GetTypeAttributes(symbol);

    var isGenericType = symbol is INamedTypeSymbol { IsGenericType: true };

    var methodInfoFactories = members
      .OfType<IMethodSymbol>()
      .Where(x => x.Name != ReflectionConstructorInfo.ConstructorMethodName)
      .Select(MethodSymbolExtensions.ToMethodInfoFactory)
      .ToArray();

    var constructorFactories = members
      .OfType<IMethodSymbol>()
      .Where(x => x.Name == ReflectionConstructorInfo.ConstructorMethodName)
      .Select(ConstructorSymbolExtensions.ToConstructorInfoFactory)
      .ToArray();

    var fieldInfoFactories = members
      .OfType<IFieldSymbol>()
      .Select(FieldSymbolExtensions.ToFieldInfoFactory)
      .ToArray();

    var propertyInfoFactories = members
      .OfType<IPropertySymbol>()
      .Select(PropertySymbolExtensions.ToPropertyInfoFactory)
      .ToArray();

    var mumeiType = ReflectionType.Create(
      symbol.Name,
      symbol.ContainingNamespace.ToDisplayString(),
      symbol.BaseType is null ? null : ToType(symbol.BaseType),
      symbol.Interfaces.Select(ToType).ToArray(),
      symbol.GetGenericArguments(),
      isGenericType,
      typeAttributes,
      methodInfoFactories,
      constructorFactories,
      fieldInfoFactories,
      propertyInfoFactories,
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
    }

    return attributes;
  }

  private static Type? GetExistingRuntimeTypeInAssemblies(string fullName) {
    foreach (var assembly in Assemblies) {
      var type = assembly.GetType(fullName);

      if (type is not null) {
        return type;
      }
    }

    return null;
  }
}
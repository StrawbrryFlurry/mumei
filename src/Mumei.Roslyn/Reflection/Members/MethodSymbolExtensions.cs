using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection.Members;

namespace Mumei.Roslyn.Reflection.Members; 

public static class MethodSymbolExtensions {
  public static MethodInfoSpec ToMethodInfoSpec(this IMethodSymbol symbol) {
    return new MethodInfoSpec {
      Name = symbol.Name,
      ReturnType = symbol.ReturnType.ToType(),
      Parameters = symbol.Parameters.Select(p => p.ToParameterInfo()).ToArray(),
      GenericArguments = symbol.TypeArguments.Select(t => t.ToType()).ToArray(),
      MethodAttributes = GetMethodAttributes(symbol),
      ImplAttributes = symbol.MethodImplementationFlags,
      CustomAttributes = symbol.GetAttributes().Select(x => x.ToAttributeDeclaration()).ToArray()
    };
  }

  private static MethodAttributes GetMethodAttributes(this IMethodSymbol symbol) {
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
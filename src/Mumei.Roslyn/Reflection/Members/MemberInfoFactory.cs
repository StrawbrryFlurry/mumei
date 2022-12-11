using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection;

internal sealed class SymbolMemberInfoFactory : IMemberInfoFactory {
  private readonly Func<Type, MemberInfo> _factoryDelegate;

  public SymbolMemberInfoFactory(ISymbol memberSymbol) {
    _factoryDelegate = memberSymbol switch {
      IFieldSymbol x => CreateFieldInfo(x),
      IPropertySymbol x => CreatePropertyInfo(x),
      IMethodSymbol x => CreateMethodInfo(x),
      _ => throw new NotSupportedException($"Symbol type {memberSymbol.Kind.ToString()} is not a type member.")
    };
  }

  public MemberInfo CreateMemberInfo(Type declaringType) {
    return _factoryDelegate(declaringType);
  }

  private Func<Type, MemberInfo> CreateMethodInfo(IMethodSymbol methodSymbol) {
    return methodSymbol.ToMethodInfoFactory().CreateMethodInfo;
  }

  private Func<Type, MemberInfo> CreatePropertyInfo(IPropertySymbol propertySymbol) {
    return propertySymbol.ToPropertyInfoFactory().CreatePropertyInfo;
  }

  private Func<Type, MemberInfo> CreateFieldInfo(IFieldSymbol fieldSymbol) {
    return fieldSymbol.ToFieldInfoFactory().CreateFieldInfo;
  }
}
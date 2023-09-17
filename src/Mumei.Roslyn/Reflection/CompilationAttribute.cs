using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public struct CompilationAttribute {
  private readonly AttributeData _attributeData;

  private INamedTypeSymbol AttributeClass => _attributeData.AttributeClass!;

  public CompilationType Type => AttributeClass.ToCompilationType();

  public CompilationAttribute(AttributeData attributeData) {
    if (attributeData.AttributeClass is null) {
      throw new InvalidOperationException("Attribute class is null");
    }

    _attributeData = attributeData;
  }

  public bool Is<TAttribute>() where TAttribute : Attribute {
    return AttributeClass?.ToCompilationType().GetFullName() == typeof(TAttribute).FullName;
  }

  public bool IsConstructedGenericTypeOf(Type unboundAttributeType) {
    if (unboundAttributeType is { IsGenericType: false } or { IsConstructedGenericType: true }) {
      throw new ArgumentException("Type must be an open generic type", nameof(unboundAttributeType));
    }

    var unboundSelfType = AttributeClass!.ConstructUnboundGenericType()!.ToCompilationType();
    return unboundSelfType.GetFullName() == unboundAttributeType.FullName;
  }

  public TArgument GetArgument<TArgument>() {
    var argument = _attributeData.ConstructorArguments[0];
    return default!;
  }
}
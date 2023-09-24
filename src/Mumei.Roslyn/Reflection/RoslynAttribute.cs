using Microsoft.CodeAnalysis;

namespace Mumei.Roslyn.Reflection;

public readonly struct RoslynAttribute {
  private readonly AttributeData _attributeData;

  private INamedTypeSymbol AttributeClass => _attributeData.AttributeClass!;

  public RoslynType Type => AttributeClass.ToCompilationType();

  public RoslynAttribute(AttributeData attributeData) {
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
      unboundAttributeType = unboundAttributeType.GetGenericTypeDefinition();
    }

    var unboundSelfType = AttributeClass!.ConstructUnboundGenericType()!.ToCompilationType();
    return unboundSelfType.GetFullName() == unboundAttributeType.FullName;
  }

  public TArgument? GetArgument<TArgument>(string propertyName, int ctorArgumentIndex) {
    return GetNamedArgument<TArgument>(propertyName) ?? GetCtorArgument<TArgument>(ctorArgumentIndex);
  }

  public TArgument? GetArgument<TArgument>(string propertyName, string ctorArgumentName) {
    return GetNamedArgument<TArgument>(propertyName) ?? GetCtorArgument<TArgument>(ctorArgumentName);
  }

  public TArgument? GetCtorArgument<TArgument>(string ctorArgumentName) {
    if (_attributeData.AttributeConstructor is null) {
      return default;
    }

    // Using a span here doesn't make much sense since the number of parameters is usually very small
    var parameters = _attributeData.AttributeConstructor.Parameters;
    for (var i = 0; i < parameters.Length; i++) {
      if (parameters[i].Name == ctorArgumentName) {
        return _attributeData.ConstructorArguments[i].GetValue<TArgument>();
      }
    }

    return default;
  }

  public TArgument? GetCtorArgument<TArgument>(int ctorArgumentIdx) {
    if (_attributeData.AttributeConstructor is null) {
      return default;
    }

    if (ctorArgumentIdx >= _attributeData.ConstructorArguments.Length) {
      return default;
    }

    var argument = _attributeData.ConstructorArguments[ctorArgumentIdx];
    return argument.GetValue<TArgument>();
  }

  public TArgument? GetNamedArgument<TArgument>(string propertyName) {
    var namedArguments = _attributeData.NamedArguments;
    for (var i = 0; i < namedArguments.Length; i++) {
      var namedArgument = namedArguments[i];
      if (namedArgument.Key == propertyName) {
        return namedArgument.Value.GetValue<TArgument>();
      }
    }

    return default;
  }
}

public static class CompilationAttributeExtensions {
  public static RoslynAttribute? GetCompilationAttribute<TAttribute>(this ITypeSymbol symbol)
    where TAttribute : Attribute {
    foreach (var attributeData in symbol.GetAttributes()) {
      var attribute = new RoslynAttribute(attributeData);
      if (attribute.Is<TAttribute>()) {
        return attribute;
      }
    }

    return null!;
  }

  public static RoslynAttribute? GetCompilationAttribute(this ITypeSymbol symbol, RoslynType attributeType) {
    foreach (var attributeData in symbol.GetAttributes()) {
      var attribute = new RoslynAttribute(attributeData);
      if (attribute.Type.GetFullName() == attributeType.GetFullName()) {
        return attribute;
      }
    }

    return null!;
  }

  public static ReadOnlySpan<RoslynAttribute> GetCompilationAttributes(this ITypeSymbol symbol) {
    var attributeDatas = symbol.GetAttributes();
    var attributes = new ArrayBuilder<RoslynAttribute>(attributeDatas.Length);
    foreach (var attributeData in attributeDatas) {
      var attribute = new RoslynAttribute(attributeData);
      attributes.Add(attribute);
    }

    return attributes.ToReadOnlySpanAndFree();
  }

  public static ReadOnlySpan<RoslynAttribute> GetCompilationAttributes<TAttribute>(this ITypeSymbol symbol)
    where TAttribute : Attribute {
    var attributes = new ArrayBuilder<RoslynAttribute>();
    foreach (var attributeData in symbol.GetAttributes()) {
      var attribute = new RoslynAttribute(attributeData);
      if (attribute.Is<TAttribute>()) {
        attributes.Add(attribute);
      }
    }

    return attributes.ToReadOnlySpanAndFree();
  }
}
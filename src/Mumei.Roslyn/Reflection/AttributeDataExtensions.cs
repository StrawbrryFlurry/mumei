﻿using System.Reflection;
using Microsoft.CodeAnalysis;
using Mumei.Common.Reflection;

namespace Mumei.Roslyn.Reflection; 

public static class AttributeDataExtensions {
  public static CustomAttributeData ToAttributeDeclaration(this AttributeData attributeData) {
    if (attributeData.AttributeClass is null) {
      throw new InvalidOperationException("Attribute class is null");
    }

    var attributeType = attributeData.AttributeClass.ToType();
    return new ReflectionCustomAttributeData(
      attributeType,
      GetAttributeConstructorArguments(attributeData.ConstructorArguments),
      GetAttributeNamedArguments(attributeType, attributeData.NamedArguments)
    );
  }

  private static IList<CustomAttributeNamedArgument> GetAttributeNamedArguments(
    Type attributeType,
    IEnumerable<KeyValuePair<string, TypedConstant>> arguments
  ) {
    var namedArguments = new List<CustomAttributeNamedArgument>();

    foreach (var argument in arguments) {
      var memberInfo = attributeType.GetMembers().First(x => x.Name == argument.Key);
      var typedValue = argument.Value.GetTypedConstantValue();
      var namedArgument = new CustomAttributeNamedArgument(memberInfo, typedValue!);
      namedArguments.Add(namedArgument);
    }

    return namedArguments;
  }

  private static List<CustomAttributeTypedArgument> GetAttributeConstructorArguments(
    IEnumerable<TypedConstant> arguments
  ) {
    var constructorArguments = new List<CustomAttributeTypedArgument>();

    foreach (var argument in arguments) {
      var value = argument.GetTypedConstantValue();
      var type = value.GetType();

      constructorArguments.Add(new CustomAttributeTypedArgument(type, value));
    }

    return constructorArguments;
  }

  private static object? GetTypedConstantValue(this TypedConstant typedConstant) {
    return typedConstant.Kind == TypedConstantKind.Array
      ? typedConstant.Values.Select(x => x.Value).ToArray()
      : typedConstant.Value;
  }
}
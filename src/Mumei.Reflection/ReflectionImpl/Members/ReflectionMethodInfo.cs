using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Mumei.Common;

internal sealed class ReflectionMethodInfo : MethodInfo {
  private static readonly ConcurrentDictionary<TypeMemberCacheKey, MethodInfo> MethodInfoCache = new();
  private readonly Type[] _genericArguments;
  private readonly ParameterInfo[] _parameters;

  private ReflectionMethodInfo(
    string name,
    Type returnType,
    ParameterInfo[] parameters,
    Type[] genericArguments,
    MethodAttributes methodAttributes,
    MethodImplAttributes implAttributes,
    CustomAttributeData[] customAttributes,
    Type declaringType
  ) {
    _genericArguments = genericArguments;
    _parameters = parameters;
    Name = name;

    DeclaringType = declaringType;
    ReflectedType = declaringType;

    ReturnType = returnType;
    ReturnTypeCustomAttributes = new MumeiCustomAttributeProvider(returnType);

    Module = declaringType.Module;
    Attributes = MethodAttributes.Abstract;

    MethodInfoCache.TryAdd(new TypeMemberCacheKey(name, declaringType), this);
  }

  public override Type DeclaringType { get; }
  public override string Name { get; }
  public override Type ReflectedType { get; }

  public override Type ReturnType { get; }
  public override Module Module { get; }

  public override MethodAttributes Attributes { get; }

  public override RuntimeMethodHandle MethodHandle =>
    throw new NotSupportedException("Cannot get a runtime handle for a compile time type");

  public override ICustomAttributeProvider ReturnTypeCustomAttributes { get; }

  public static MethodInfo Create(
    string name,
    Type returnType,
    ParameterInfo[] parameters,
    Type[] genericArguments,
    MethodAttributes methodAttributes,
    MethodImplAttributes implAttributes,
    CustomAttributeData[] customAttributes,
    Type declaringType
  ) {
    var key = new TypeMemberCacheKey(name, declaringType);
    return MethodInfoCache.GetOrAdd(
      key,
      _ => new ReflectionMethodInfo(
        name,
        returnType,
        parameters,
        genericArguments,
        methodAttributes,
        implAttributes,
        customAttributes,
        declaringType
      )
    );
  }

  public override object[] GetCustomAttributes(bool inherit) {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
    throw new NotSupportedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit) {
    throw new NotSupportedException();
  }

  public override MethodImplAttributes GetMethodImplementationFlags() {
    throw new NotSupportedException();
  }

  public override ParameterInfo[] GetParameters() {
    return _parameters;
  }

  public override object Invoke(
    object? obj,
    BindingFlags invokeAttr,
    Binder? binder,
    object?[]? parameters,
    CultureInfo? culture
  ) {
    throw new NotSupportedException("Cannot invoke a compile time method.");
  }

  public override MethodInfo GetBaseDefinition() {
    return this;
  }
}
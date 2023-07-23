using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Mumei.Common;

internal sealed class ReflectionPropertyInfo : PropertyInfo {
  private static readonly ConcurrentDictionary<TypeMemberCacheKey, PropertyInfo> PropertyInfoCache = new();
  private readonly ParameterInfo[] _indexParameters;
  private readonly bool _isIndexer;

  private ReflectionPropertyInfo(
    string name,
    Type propertyType,
    MethodInfo getMethod,
    MethodInfo? setMethod,
    PropertyAttributes propertyAttributes,
    bool isIndexer,
    ParameterInfo[] indexParameters,
    Type declaringType
  ) {
    DeclaringType = declaringType;
    Name = name;
    PropertyType = propertyType;
    CanRead = true;
    CanWrite = setMethod is not null;
    GetMethod = getMethod;

    if (SetMethod is not null) {
      SetMethod = setMethod;
    }

    _isIndexer = isIndexer;
    _indexParameters = indexParameters;

    Attributes = propertyAttributes;
    PropertyInfoCache.TryAdd(new TypeMemberCacheKey(Name, DeclaringType), this);
  }

  public override Type DeclaringType { get; }
  public override string Name { get; }
  public override Type ReflectedType { get; } = null!;

  public override PropertyAttributes Attributes { get; }

  public override bool CanRead { get; }
  public override bool CanWrite { get; }
  public override Type PropertyType { get; }

  public override MethodInfo GetMethod { get; }
  public override MethodInfo? SetMethod { get; }

  public static PropertyInfo Create(
    string name,
    Type propertyType,
    MethodInfo getMethod,
    MethodInfo? setMethod,
    PropertyAttributes propertyAttributes,
    bool isIndexer,
    ParameterInfo[] indexParameters,
    Type declaringType
  ) {
    var key = new TypeMemberCacheKey(name, declaringType);
    return PropertyInfoCache.GetOrAdd(
      key,
      _ => new ReflectionPropertyInfo(
        name,
        propertyType,
        getMethod,
        setMethod,
        propertyAttributes,
        isIndexer,
        indexParameters,
        declaringType
      )
    );
  }

  public override object[] GetCustomAttributes(bool inherit) {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit) {
    throw new NotImplementedException();
  }

  public override MethodInfo[] GetAccessors(bool nonPublic) {
    throw new NotImplementedException();
  }

  public override MethodInfo GetGetMethod(bool nonPublic) {
    return GetMethod;
  }

  public override ParameterInfo[] GetIndexParameters() {
    return _isIndexer
      ? _indexParameters
      : Array.Empty<ParameterInfo>();
  }

  public override MethodInfo? GetSetMethod(bool nonPublic) {
    return nonPublic
      ? SetMethod
      : SetMethod?.IsPublic is true
        ? SetMethod
        : null;
  }

  public override object GetValue(
    object? obj,
    BindingFlags invokeAttr,
    Binder? binder,
    object?[]? index,
    CultureInfo? culture
  ) {
    throw new NotSupportedException("Cannot get value of a compile time property.");
  }

  public override void SetValue(
    object? obj,
    object? value,
    BindingFlags invokeAttr,
    Binder? binder,
    object?[]? index,
    CultureInfo? culture
  ) {
    throw new NotSupportedException("Cannot set value of a compile time property.");
  }
}
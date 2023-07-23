using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Mumei.Common;

internal sealed class ReflectionFieldInfo : FieldInfo {
  private static readonly ConcurrentDictionary<TypeMemberCacheKey, ReflectionFieldInfo> FieldInfoCache = new();
  private readonly ReflectionAttributeCollection _customAttributeData;

  private ReflectionAttributeSearcher<FieldInfo>? _attributeSearcher;

  private ReflectionFieldInfo(
    string name,
    Type fieldType,
    FieldAttributes fieldAttributes,
    ReflectionAttributeCollection customAttributeData,
    Type declaringType
  ) {
    _customAttributeData = customAttributeData;
    DeclaringType = declaringType;
    Name = name;
    FieldType = fieldType;
    Attributes = fieldAttributes;

    FieldInfoCache.TryAdd(new TypeMemberCacheKey(name, declaringType), this);
  }

  public override IEnumerable<CustomAttributeData> CustomAttributes => _customAttributeData;
  public override Module Module => DeclaringType.Module;

  public override Type DeclaringType { get; }
  public override string Name { get; }
  public override Type ReflectedType => throw new NotSupportedException();

  public override FieldAttributes Attributes { get; }
  public override RuntimeFieldHandle FieldHandle => throw new NotSupportedException();
  public override Type FieldType { get; }

  public static FieldInfo Create(
    string name,
    Type fieldType,
    FieldAttributes fieldAttributes,
    ReflectionAttributeCollection customAttributeData,
    Type declaringType
  ) {
    var key = new TypeMemberCacheKey(name, declaringType);
    return FieldInfoCache.GetOrAdd(
      key,
      _ => new ReflectionFieldInfo(
        name,
        fieldType,
        fieldAttributes,
        customAttributeData,
        declaringType
      )
    );
  }

  public override IList<CustomAttributeData> GetCustomAttributesData() {
    return _customAttributeData.Clone();
  }

  public override object[] GetCustomAttributes(bool inherit) {
    _attributeSearcher ??= new ReflectionAttributeSearcher<FieldInfo>(this);
    return _attributeSearcher.GetCustomAttributes(DeclaringType, inherit);
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
    _attributeSearcher ??= new ReflectionAttributeSearcher<FieldInfo>(this);
    return _attributeSearcher.GetCustomAttributes(attributeType, DeclaringType, inherit);
  }

  public override bool IsDefined(Type attributeType, bool inherit) {
    _attributeSearcher ??= new ReflectionAttributeSearcher<FieldInfo>(this);
    return _attributeSearcher.IsDefined(attributeType, inherit);
  }

  public override object GetValue(object? obj) {
    throw new NotSupportedException();
  }

  public override void SetValue(
    object? obj,
    object? value,
    BindingFlags invokeAttr,
    Binder? binder,
    CultureInfo? culture
  ) {
    throw new NotSupportedException();
  }

  public override object? GetRawConstantValue() {
    throw new NotSupportedException();
  }
}
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class ReflectionFieldInfo : FieldInfo {
  private static readonly ConcurrentDictionary<TypeMemberCacheKey, ReflectionFieldInfo> FieldInfoCache = new();
  private readonly IList<CustomAttributeData> _customAttributeData;

  private ReflectionFieldInfo(
    string name,
    Type fieldType,
    FieldAttributes fieldAttributes,
    IList<CustomAttributeData> customAttributeData,
    Type declaringType
  ) {
    _customAttributeData = customAttributeData;
    DeclaringType = declaringType;
    Name = name;
    FieldType = fieldType;
    Attributes = fieldAttributes;

    FieldInfoCache.TryAdd(new TypeMemberCacheKey(name, declaringType), this);
  }

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
    IList<CustomAttributeData> customAttributeData,
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
    return _customAttributeData;
  }


  public override object[] GetCustomAttributes(bool inherit) {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit) {
    return _customAttributeData.Any(x =>
      x.AttributeType.FullName == attributeType.FullName
      && inherit
      && x.AttributeType.DeclaringType == DeclaringType
    );
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
}
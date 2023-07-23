using System.Collections.Concurrent;
using System.Reflection;

namespace Mumei.Common;

internal sealed class ReflectionParameterInfo : ParameterInfo {
  private static readonly ConcurrentDictionary<ParameterInfoKey, ParameterInfo> ParameterInfoCache = new();
  private readonly ReflectionAttributeCollection _customAttributes;
  private readonly IMemberInfoFactory _declaringMemberFactory;
  private readonly Type _declaringType;

  private ReflectionParameterInfo(
    string name,
    Type parameterType,
    Type declaringType,
    IMemberInfoFactory declaringMemberFactory,
    ReflectionAttributeCollection customAttributes,
    int position,
    bool hasDefaultValue,
    object? defaultValue
  ) {
    Name = name;
    Position = position;
    ParameterType = parameterType;
    HasDefaultValue = hasDefaultValue;
    DefaultValue = defaultValue;
    _declaringType = declaringType;
    _declaringMemberFactory = declaringMemberFactory;
    _customAttributes = customAttributes;

    var key = new ParameterInfoKey(declaringType, declaringMemberFactory.MemberInfoName, name, position);
    ParameterInfoCache.TryAdd(key, this);
  }

  public override MemberInfo Member => _declaringMemberFactory.CreateMemberInfo(_declaringType);

  public override Type ParameterType { get; }

  public override string Name { get; }

  public override bool HasDefaultValue { get; }

  public override object? DefaultValue { get; }

  public override int Position { get; }

  public static ParameterInfo Create(
    string name,
    Type declaringType,
    IMemberInfoFactory declaringMemberFactory,
    Type parameterType,
    ReflectionAttributeCollection customAttributes,
    int position,
    bool hasDefaultValue,
    object? defaultValue
  ) {
    var key = new ParameterInfoKey(declaringType, declaringMemberFactory.MemberInfoName, name, position);

    return ParameterInfoCache.GetOrAdd(
      key,
      _ => new ReflectionParameterInfo(
        name,
        parameterType,
        declaringType,
        declaringMemberFactory,
        customAttributes,
        position,
        hasDefaultValue,
        defaultValue
      )
    );
  }

  public override IList<CustomAttributeData> GetCustomAttributesData() {
    return _customAttributes.Clone();
  }

  private record struct ParameterInfoKey(Type DeclaringType, string MemberInfoName, string Name, int Position);
}
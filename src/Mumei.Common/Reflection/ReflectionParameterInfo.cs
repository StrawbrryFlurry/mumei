using System.Reflection;

namespace Mumei.Common.Reflection;

public sealed class ReflectionParameterInfo : ParameterInfo {
  private readonly List<CustomAttributeData> _customAttributes;

  internal ReflectionParameterInfo(
    string name,
    Type parameterType,
    List<CustomAttributeData> customAttributes,
    int position,
    bool hasDefaultValue,
    object? defaultValue
  ) {
    Name = name;
    Position = position;
    ParameterType = parameterType;
    HasDefaultValue = hasDefaultValue;
    DefaultValue =  defaultValue;
    _customAttributes = customAttributes;
  }


  public override Type ParameterType { get; }

  public override string Name { get; }

  public override bool HasDefaultValue { get; }

  public override object? DefaultValue { get; }

  public override int Position { get; }

  public override IList<CustomAttributeData> GetCustomAttributesData() {
    return _customAttributes;
  }
}

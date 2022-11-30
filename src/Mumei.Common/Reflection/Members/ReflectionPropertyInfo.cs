using System.Globalization;
using System.Reflection;

namespace Mumei.Common.Reflection.Members;

public sealed class ReflectionPropertyInfo : PropertyInfo {
  private readonly ParameterInfo[] _indexParameters;
  private readonly bool _isIndexer;

  internal ReflectionPropertyInfo(PropertyInfoSpec spec, Type declaringType) {
    DeclaringType = declaringType;
    Name = spec.Name;
    PropertyType = spec.PropertyType;
    CanRead = spec.CanRead;
    CanWrite = spec.CanWrite;
    GetMethod = new ReflectionMethodInfo(spec.GetMethod, declaringType);

    if (spec.SetMethod is not null) {
      SetMethod = new ReflectionMethodInfo(spec.SetMethod.Value, declaringType);
    }

    _isIndexer = spec.IsIndexer;
    _indexParameters = spec.IndexParameters;
  }

  public override Type DeclaringType { get; }
  public override string Name { get; }
  public override Type ReflectedType { get; }

  public override PropertyAttributes Attributes { get; }

  public override bool CanRead { get; }
  public override bool CanWrite { get; }
  public override Type PropertyType { get; }

  public MethodInfo GetMethod { get; }
  public MethodInfo? SetMethod { get; }

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
    object obj,
    BindingFlags invokeAttr,
    Binder binder,
    object[] index,
    CultureInfo culture
  ) {
    throw new NotSupportedException("Cannot get value of a compile time property.");
  }

  public override void SetValue(
    object obj,
    object value,
    BindingFlags invokeAttr,
    Binder binder,
    object[] index,
    CultureInfo culture
  ) {
    throw new NotSupportedException("Cannot set value of a compile time property.");
  }
}

public struct PropertyInfoSpec {
  public string Name { get; set; }
  public Type PropertyType { get; set; }
  public bool CanRead { get; set; }
  public bool CanWrite { get; set; }
  public MethodInfoSpec GetMethod { get; set; }
  public MethodInfoSpec? SetMethod { get; set; }

  public bool IsIndexer { get; set; }
  public ParameterInfo[] IndexParameters { get; set; }
}
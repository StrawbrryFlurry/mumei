using System.Globalization;
using System.Reflection;

namespace Mumei.Common.Reflection.Members;

public sealed class ReflectionFieldInfo : FieldInfo {
  internal ReflectionFieldInfo(FieldInfoSpec spec, Type declaringType) {
    DeclaringType = declaringType;
    Name = spec.Name;
    FieldType = spec.FieldType;
    Attributes = spec.Attributes;
  }

  public override Type DeclaringType { get; }
  public override string Name { get; }
  public override Type ReflectedType => throw new NotSupportedException();

  public override FieldAttributes Attributes { get; }
  public override RuntimeFieldHandle FieldHandle => throw new NotSupportedException();
  public override Type FieldType { get; }

  public override object[] GetCustomAttributes(bool inherit) {
    throw new NotImplementedException();
  }

  public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
    throw new NotImplementedException();
  }

  public override bool IsDefined(Type attributeType, bool inherit) {
    throw new NotImplementedException();
  }

  public override object GetValue(object? obj) {
    throw new NotSupportedException();
  }

  public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder,
    CultureInfo? culture) {
    throw new NotSupportedException();
  }
}

public struct FieldInfoSpec {
  public string Name { get; set; }
  public Type FieldType { get; set; }
  public FieldAttributes Attributes { get; set; }
}
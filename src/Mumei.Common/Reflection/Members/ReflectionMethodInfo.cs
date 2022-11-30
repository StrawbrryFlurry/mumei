using System.Globalization;
using System.Reflection;

namespace Mumei.Common.Reflection.Members;

public sealed class ReflectionMethodInfo : MethodInfo {
  private readonly Type[] _genericArguments;
  private readonly ParameterInfo[] _parameters;

  internal ReflectionMethodInfo(
    MethodInfoSpec spec,
    Type declaringType
  ) {
    _genericArguments = spec.GenericArguments;
    _parameters = spec.Parameters;
    Name = spec.Name;

    DeclaringType = declaringType;
    ReflectedType = declaringType;

    ReturnType = spec.ReturnType;
    ReturnTypeCustomAttributes = new MumeiCustomAttributeProvider(spec.ReturnType);

    Module = declaringType.Module;
    Attributes = MethodAttributes.Abstract;
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
    object obj,
    BindingFlags invokeAttr,
    Binder binder,
    object[] parameters,
    CultureInfo culture
  ) {
    throw new NotSupportedException("Cannot invoke a compile time method.");
  }

  public override MethodInfo GetBaseDefinition() {
    return this;
  }
}

public struct MethodInfoSpec {
  public string Name { get; set; }
  public Type ReturnType { get; set; }
  public ParameterInfo[] Parameters { get; set; }
  public Type[] GenericArguments { get; set; }
  public MethodAttributes MethodAttributes { get; set; }
  public MethodImplAttributes ImplAttributes { get; set; }
  public CustomAttributeData[] CustomAttributes { get; set; }
}

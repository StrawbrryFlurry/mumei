using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class ReflectionConstructorInfo : ConstructorInfo {
  public const string ConstructorMethodName = ".ctor";
  private static readonly ConcurrentDictionary<TypeMemberCacheKey, ConstructorInfo> ConstructorInfoCache = new();

  private ReflectionConstructorInfo(string name, MethodAttributes methodAttributes, Type declaringType) {
    Name = name;
    DeclaringType = declaringType;
    Attributes = methodAttributes;
  }

  public override Type DeclaringType { get; }
  public override string Name { get; }
  public override Type? ReflectedType { get; }

  public override MethodAttributes Attributes { get; }
  public override RuntimeMethodHandle MethodHandle { get; }

  public static ConstructorInfo Create(string name, MethodAttributes methodAttributes, Type declaringType) {
    var key = new TypeMemberCacheKey(name, declaringType);

    return ConstructorInfoCache.GetOrAdd(
      key,
      _ => new ReflectionConstructorInfo(name, methodAttributes, declaringType)
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

  public override MethodImplAttributes GetMethodImplementationFlags() {
    throw new NotImplementedException();
  }

  public override ParameterInfo[] GetParameters() {
    throw new NotImplementedException();
  }

  public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters,
    CultureInfo? culture) {
    throw new NotImplementedException();
  }

  public override object Invoke(BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture) {
    throw new NotImplementedException();
  }
}
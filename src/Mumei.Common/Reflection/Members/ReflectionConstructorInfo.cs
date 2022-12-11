using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Mumei.Common.Reflection;

internal sealed class ReflectionConstructorInfo : ConstructorInfo {
  private static readonly ConcurrentDictionary<TypeMemberCacheKey, ConstructorInfo> ConstructorInfoCache = new();

  private ReflectionConstructorInfo(ConstructorInfoSpec spec, Type declaringType) {
    Name = spec.Name;
    DeclaringType = declaringType;

    ConstructorInfoCache.TryAdd(new TypeMemberCacheKey(spec.Name, declaringType), this);
  }

  public override Type DeclaringType { get; }
  public override string Name { get; }
  public override Type? ReflectedType { get; }

  public override MethodAttributes Attributes { get; }
  public override RuntimeMethodHandle MethodHandle { get; }

  public static ConstructorInfo Create(ConstructorInfoSpec spec, Type declaringType) {
    var key = new TypeMemberCacheKey(spec.Name, declaringType);

    return ConstructorInfoCache.GetOrAdd(
      key,
      _ => new ReflectionConstructorInfo(spec, declaringType)
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

internal struct ConstructorInfoSpec {
  public string Name { get; }
}
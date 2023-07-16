using System.Reflection;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers.Resolution;

namespace Mumei.DependencyInjection.Providers.Dynamic.Resolution;

public static class DynamicSingletonBinding {
  [ThreadStatic]
  internal static object[]? CtorCallArgs;

  internal static readonly Type[] CtorArgTypes = { typeof(object) };

  public static Binding CreateDynamic(
    Type bindingType,
    object instance
  ) {
    var ctorCallArgs = CtorCallArgs ??= new object[1];
    ctorCallArgs[0] = instance;

    return (Binding)GetConstructorForProviderType(bindingType).Invoke(ctorCallArgs);
  }

  private static ConstructorInfo GetConstructorForProviderType(Type providerType) {
    return typeof(DynamicSingletonBinding<>)
      .MakeGenericType(providerType)
      .GetConstructor(CtorArgTypes)!;
  }
}

public sealed class DynamicSingletonBinding<TProvider> : SingletonBinding<TProvider> {
  private readonly TProvider _instance;

  public DynamicSingletonBinding(object instance) {
    _instance = (TProvider)instance;
  }

  protected internal override TProvider Create(IInjector? scope = null) {
    return _instance;
  }
}
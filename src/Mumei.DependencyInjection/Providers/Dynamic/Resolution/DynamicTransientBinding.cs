﻿using System.Reflection;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers.Resolution;

namespace Mumei.DependencyInjection.Providers.Dynamic.Resolution;

public static class DynamicTransientBinding {
  [ThreadStatic]
  internal static object[]? CtorCallArgs;

  internal static readonly Type[] CtorArgTypes = { typeof(DynamicProviderFactory), typeof(IInjector) };

  public static Binding CreateDynamic(
    Type bindingType,
    DynamicProviderFactory factory,
    IInjector injector
  ) {
    var ctorCallArgs = CtorCallArgs ??= new object[2];
    ctorCallArgs[0] = factory;
    ctorCallArgs[1] = injector;

    return (Binding)GetConstructorForProviderType(bindingType).Invoke(ctorCallArgs);
  }

  private static ConstructorInfo GetConstructorForProviderType(Type providerType) {
    return typeof(DynamicTransientBinding<>)
      .MakeGenericType(providerType)
      .GetConstructor(CtorArgTypes)!;
  }
}

public sealed class DynamicTransientBinding<TProvider> : TransientBinding<TProvider> {
  private readonly DynamicProviderFactory _factory;
  private readonly IInjector _injector;

  public DynamicTransientBinding(DynamicProviderFactory factory, IInjector injector) {
    _factory = factory;
    _injector = injector;
  }

  protected internal override TProvider Create(IInjector? scope = null) {
    return (TProvider)_factory(_injector, scope);
  }
}
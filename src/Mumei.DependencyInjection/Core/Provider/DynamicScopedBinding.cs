﻿using System.Reflection;

namespace Mumei.DependencyInjection.Core;

public static class DynamicScopedBinding {
  [ThreadStatic]
  internal static object[]? CtorCallArgs;

  internal static readonly Type[] CtorArgTypes = { typeof(ProviderFactory), typeof(IInjector) };

  public static Binding CreateDynamic(
    Type bindingType,
    ProviderFactory factory,
    IInjector injector
  ) {
    var ctorCallArgs = CtorCallArgs ??= new object[2];
    ctorCallArgs[0] = factory;
    ctorCallArgs[1] = injector;

    return (Binding)GetConstructorForProviderType(bindingType).Invoke(ctorCallArgs);
  }

  private static ConstructorInfo GetConstructorForProviderType(Type providerType) {
    return typeof(DynamicScopedBinding<>)
      .MakeGenericType(providerType)
      .GetConstructor(CtorArgTypes)!;
  }
}

public sealed class DynamicScopedBinding<TProvider> : ScopedBinding<TProvider> {
  private readonly ProviderFactory _factory;
  private readonly IInjector _injector;

  public DynamicScopedBinding(
    ProviderFactory factory,
    IInjector injector
  ) {
    _factory = factory;
    _injector = injector;
  }

  protected internal override TProvider Create(IInjector? scope = null) {
    return (TProvider)_factory(_injector, scope);
  }
}
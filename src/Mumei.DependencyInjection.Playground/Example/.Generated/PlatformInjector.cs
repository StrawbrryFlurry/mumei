﻿using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Playground.Example.Generated;

internal sealed partial class PlatformInjector : IInjector {
  public static readonly PlatformInjector Instance = new();

  private PlatformInjector() { }

  public IInjector Parent { get; } = NullInjector.Instance;

  public static EnvironmentInjector<TAppModule> CreateEnvironment<TAppModule>() {
    return typeof(TAppModule) switch {
      _ when typeof(TAppModule) == typeof(IApplicationModule) => (dynamic)new ApplicationModuleλEnvironmentInjector()!,
      _ => ModuleNotFound<TAppModule>()
    };
  }

  private static object ModuleNotFound<TModule>() {
    throw new InvalidOperationException(
      $"The module {typeof(TModule).FullName} was not found!" +
      "Please ensure that the module is declared as a root module by the [RootModule] Attribute " +
      "or is explicitly requested trough: PlatformInjector.CreateEnvironment<TRootModule>()"
    );
  }
}

internal sealed partial class PlatformInjector {
  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    var provider = typeof(TProvider);
    var instance = provider switch {
      _ when provider == typeof(IInjector) => Instance,
      _ => Parent.Get(provider)
    };

    return (TProvider)instance;
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    return token switch {
      Type t when t == typeof(IInjector) => Instance,
      _ => Parent.Get(token)
    };
  }
}
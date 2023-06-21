using Mumei.DependencyInjection.Core;
using Mumei.Playground.SimpleApplication;

namespace WeatherApplication.Generated;

internal sealed partial class PlatformInjector : IInjector {
  public static readonly PlatformInjector Instance = new();

  private PlatformInjector() { }

  public IInjector Parent { get; } = NullInjector.Instance;

  public static ApplicationEnvironment<TAppModule> CreateEnvironment<TAppModule>() where TAppModule : IModule {
    return typeof(TAppModule) switch {
      _ when typeof(TAppModule) == typeof(IApplicationModule) =>
        (dynamic)new ApplicationModuleλApplicationEnvironment()!,
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
  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    var provider = typeof(TProvider);
    var instance = provider switch {
      _ when provider == typeof(IInjector) => Instance,
      _ => Parent.Get(provider)
    };

    return (TProvider)instance;
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return token switch {
      Type t when t == typeof(IInjector) => Instance,
      _ => Parent.Get(token)
    };
  }
}
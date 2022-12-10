using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Playground.Example.Generated;

internal sealed partial class PlatformInjector : IInjector {
  public static readonly PlatformInjector Instance = new();

  private PlatformInjector() { }

  public IInjector Parent { get; } = NullInjector.Instance;

  public static EnvironmentInjector<TAppModule> CreateEnvironment<TAppModule>() where TAppModule : IModule {
    return new EnvironmentInjector<TAppModule>();
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
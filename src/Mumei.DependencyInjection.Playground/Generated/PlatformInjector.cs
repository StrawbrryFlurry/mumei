using Mumei.Core;

namespace Mumei.DependencyInjection.Playground.Generated;

public partial class PlatformInjector : IInjector {
  public static readonly PlatformInjector Instance = new();

  private PlatformInjector() { }

  public IInjector Parent { get; } = EnvironmentInjector.Instance;

  public partial T Get<T>();
  public partial object Get(Type provider);
}

public partial class PlatformInjector {
  public partial T Get<T>() {
    var provider = typeof(T);
    var instance = provider switch {
      _ when provider == typeof(IInjector) => Instance,
      _ => Parent.Get(provider)
    };

    return (T)instance;
  }

  public partial object Get(Type provider) {
    return provider switch {
      _ when provider == typeof(IInjector) => Instance,
      _ => Parent.Get(provider)
    };
  }
}
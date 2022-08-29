using Mumei.Core;
using Mumei.Core.Attributes;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Weather;

namespace Mumei.DependencyInjection.Playground;

[Module]
[ApplicationRoot]
public partial class AppModule {
  [Import<WeatherModule>]
  [Import<CommonModule>]
  partial void Imports();
}

public partial class AppModule : IApplicationModule, IInjector {
  public AppModule(WeatherModule weatherModule, CommonModule commonModule) {
    WeatherModule = weatherModule;
    CommonModule = commonModule;
  }

  public WeatherModule WeatherModule { get; }
  public CommonModule CommonModule { get; }

  public ComponentRef<object>[] Components { get; }

  public T Get<T>() {
    throw new NotImplementedException();
  }

  public object Get(Type provider) {
    return provider switch {
      _ when provider == typeof(IHttpClient) => CommonModule.HttpClient,
      _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
    };
  }

  public IModule[] Modules { get; }

  public IInjector Parent { get; }

  partial void Imports() { }
}
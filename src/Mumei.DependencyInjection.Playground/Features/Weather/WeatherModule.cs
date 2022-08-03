using Mumei.Core;
using Mumei.Core.Attributes;

namespace Mumei.DependencyInjection.Playground.Weather;

[Module]
public partial class WeatherModule {
  [Singleton<IWeatherService, WeatherService>]
  partial void Providers();

  [Component<WeatherController>]
  partial void Controllers();
}

// Generated:
public partial class WeatherModule : IModule {
  private Binding<IWeatherService> __IWeatherService_WeatherService__;
  public IWeatherService WeatherService => __IWeatherService_WeatherService__.Get();

  public T Get<T>() {
    return (T)Get(typeof(T));
  }

  public object Get(Type provider) {
    return provider switch {
      _ when provider == typeof(IWeatherService) => __IWeatherService_WeatherService__.Get(),
      _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
    };
  }

  partial void Providers() { }
  partial void Controllers() { }
  private void Imports() { }
}
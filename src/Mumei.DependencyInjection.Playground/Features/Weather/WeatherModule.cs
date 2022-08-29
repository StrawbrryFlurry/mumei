using Mumei.Core;
using Mumei.Core.Attributes;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Weather.generated;

namespace Mumei.DependencyInjection.Playground.Weather;

[Module]
public partial class WeatherModule {
  [Singleton<IWeatherService, WeatherService>]
  partial void Providers();

  [Component<WeatherController>]
  partial void Controllers();

  [Import<CommonModule>]
  partial void Imports();

  [Configure<IWeatherService>]
  public void ConfigureWeatherService(ref IHttpClient httpClient) {
    httpClient.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");
  }
}

// Generated:

public partial class WeatherModule : IModule {
  private readonly Binding<WeatherController> _weatherController;
  public readonly Binding<IWeatherService> WeatherService;

  public WeatherModule(CommonModule commonModule) {
    CommonModule = commonModule;
    WeatherService = new SingletonBinding<IWeatherService>(new WeatherServiceλλFactory());
    _weatherController = new SingletonBinding<WeatherController>(new WeatherControllerλλFactory(WeatherService));
  }

  public CommonModule CommonModule { get; }

  public ComponentRef<object>[] Components { get; }

  public T Get<T>() {
    return (T)Get(typeof(T));
  }

  public object Get(Type provider) {
    return provider switch {
      _ when provider == typeof(IWeatherService) => WeatherService.Get(),
      _ when provider == typeof(WeatherController) => _weatherController.Get(),
      _ when provider == typeof(IHttpClient) => CommonModule.HttpClient,
      _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
    };
  }

  partial void Providers() { }
  partial void Controllers() { }
  partial void Imports() { }
}
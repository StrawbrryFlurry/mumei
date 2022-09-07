using Mumei.Core.Attributes;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;

namespace Mumei.DependencyInjection.Playground.Example.Modules;

[Module]
[Import<CommonModule>]
[Component<WeatherController>]
public interface IWeatherModule {
  [Transient<WeatherService>]
  public IWeatherService WeatherService { get; }

  [ConfigureFor<IWeatherService>]
  public IHttpClient ConfigureHttpClient(IHttpClient httpClient) {
    httpClient.BaseAddress = new Uri("https://api.openweathermap.org");
    return httpClient;
  }
}
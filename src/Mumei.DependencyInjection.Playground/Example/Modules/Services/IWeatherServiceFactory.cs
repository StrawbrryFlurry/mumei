using Mumei.Core;
using Mumei.DependencyInjection.Playground.Common;

namespace Mumei.DependencyInjection.Playground.Example.Modules.Services;

public sealed class IWeatherServiceλFactory : IProviderFactory<IWeatherService> {
  private readonly IProviderFactory<IHttpClient> _httpClient;

  public IWeatherServiceλFactory(IProviderFactory<IHttpClient> httpClient) {
    _httpClient = httpClient;
  }

  public IWeatherService Get() {
    return new WeatherService(_httpClient.Get());
  }
}
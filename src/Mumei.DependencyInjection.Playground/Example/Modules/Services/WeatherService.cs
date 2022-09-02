using Mumei.DependencyInjection.Playground.Common;

namespace Mumei.DependencyInjection.Playground.Example.Modules.Services;

public class WeatherService : IWeatherService {
  private readonly IHttpClient _httpClient;

  public WeatherService(IHttpClient httpClient) {
    _httpClient = httpClient;
  }

  public string GetWeather() {
    throw new NotImplementedException();
  }
}
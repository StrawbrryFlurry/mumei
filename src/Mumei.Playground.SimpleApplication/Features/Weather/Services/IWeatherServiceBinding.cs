using Mumei.DependencyInjection.Core;

namespace WeatherApplication.Features.Weather.Services;

public sealed class WeatherServiceλBinding : ScopedBinding<IWeatherService> {
  private readonly Binding<HttpClient> _httpClientBinding;
  private readonly Binding<ILogger<WeatherService>> _loggerWeatherServiceBinding;

  public WeatherServiceλBinding(
    Binding<HttpClient> httpClientBinding,
    Binding<ILogger<WeatherService>> loggerWeatherServiceBinding
  ) {
    _httpClientBinding = httpClientBinding;
    _loggerWeatherServiceBinding = loggerWeatherServiceBinding;
  }

  protected override IWeatherService Create(IInjector? scope = null) {
    return new WeatherService(
      _loggerWeatherServiceBinding.Get(scope),
      _httpClientBinding.Get(scope)
    );
  }
}
using WeatherApplication.Features.Weather.Services;

namespace WeatherApplication.Features.Weather.Controllers;

public sealed class WeatherController {
  public WeatherController(IWeatherService weatherService) { }
}
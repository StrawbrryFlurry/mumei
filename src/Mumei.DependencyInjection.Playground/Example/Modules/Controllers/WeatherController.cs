using Mumei.Core;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;

namespace Mumei.DependencyInjection.Playground.Example.Modules;

public class WeatherController {
  public WeatherController(IWeatherService weatherService, [Optional] OptionalService? optionalService) { }
}
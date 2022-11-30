using Mumei.Core;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;

namespace Mumei.DependencyInjection.Playground.Example.Modules;

public sealed class WeatherController : IComponent {
  public WeatherController(IWeatherService weatherService) { }
}
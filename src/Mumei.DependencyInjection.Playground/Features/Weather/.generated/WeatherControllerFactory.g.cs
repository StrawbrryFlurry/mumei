using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mumei.Core;

namespace Mumei.DependencyInjection.Playground.Weather.generated; 

public class WeatherControllerλλFactory : Provider<WeatherController> {
  private readonly Binding<IWeatherService> weatherService;

  public WeatherControllerλλFactory(Binding<IWeatherService> weatherService) {
    this.weatherService = weatherService;
  }

  public override WeatherController Get() {
    return new WeatherController(weatherService.Get());
  }
}
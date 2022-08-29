using Mumei.Core;

namespace Mumei.DependencyInjection.Playground.Weather.generated; 

public class WeatherServiceλλFactory : Provider<IWeatherService>{
  public override IWeatherService Get() {
    return new WeatherService();
  }
}
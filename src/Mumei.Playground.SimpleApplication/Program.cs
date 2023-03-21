using WeatherApplication.Features.Weather.Services;
using WeatherApplication.Generated;

namespace WeatherApplication;

public class Program {
  public static void Main() {
    var appEnvironment = PlatformInjector.CreateEnvironment<IApplicationModule>();
    var weatherService = appEnvironment.Module.Get<IWeatherService>();

    var weather = weatherService.GetWeather();
  }
}
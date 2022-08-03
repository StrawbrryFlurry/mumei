using Mumei.Core;

namespace Mumei.DependencyInjection.Playground;

public class Program {
  public static void Main() {
    var app = new ApplicationBuilder<AppModule>()
      .Build();
    var s = app.Weather_WeatherService;
  }
}
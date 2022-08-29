using Mumei.Core;

namespace Mumei.DependencyInjection.Playground;

public class Program {
  public static void Main() {
    var app = new ApplicationBuilder<AppModule>()
      .Build();

    var s = app.WeatherModule.WeatherService.Get();
  }

  private void MapControllers(IApplicationModule module) {
    foreach (var moduleRef in module.Modules) {
      foreach (var componentRef in moduleRef.Components) { }
    }
  }
}
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;

namespace Mumei.DependencyInjection.Playground.Example;

public class Program {
  public static void Main() {
    //var app = new ApplicationBuilder<ApplicationModule>().Build();
    var common = new CommonModule();
    var weather = new WeatherModule(null, common);
    var app = new ApplicationModule(weather, common);

    var wa = app.WeatherServiceBinding.Get();
    var s1 = app.WeatherModule.CreateScope();
    var s2 = app.WeatherModule.CreateScope();

    var w1 = s1.Get<IWeatherService>();
    Console.Write(w1);
    var w2 = s2.Get<IWeatherService>();
    Console.Write(w2);
    Console.Write(wa == w1);
    Console.Write(w1 == w2);
  }

  // private void MapControllers(IApplicationModule module) {
  //   foreach (IModuleRef moduleRef in module.Modules) {
  //     foreach (IComponentRef componentRef in moduleRef.Components) {
  //        ... Get Routing metadata
  //     }
  //   }
  // }
}
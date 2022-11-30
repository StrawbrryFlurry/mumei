using System.Reflection;
using Castle.Core.Internal;
using Mumei.Core;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Generated;
using Mumei.DependencyInjection.Playground.Example.Modules;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;
using Mumei.DependencyInjection.Playground.Lib.Http;

namespace Mumei.DependencyInjection.Playground.Example;

public class Program {
  public static void Main() {
    var appEnvironment = PlatformInjector.CreateEnvironment<IApplicationModule>();
    
    ScanModules(appEnvironment.Instance);
    
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

  private static void ScanModules(IModuleRef<IModule> module) {
    foreach (var moduleRef in module.Imports) {
      foreach (var componentRef in moduleRef.Components) {
        var controllerAttribute = componentRef.Type.GetAttribute<ControllerAttribute>();

        if (controllerAttribute is not null) {
          MapController(componentRef);
        }
      }
    }
  }

  private static void MapController(IComponentRef componentRef) {
    var route = componentRef.Type.GetAttribute<RouteAttribute>().Path;
    var methods = componentRef.Type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

    foreach (var method in methods) {
      var getAttribute = method.GetAttribute<HttpGetAttribute>();

      if (getAttribute is null) {
        continue;
      }

      var handlerDescriptor = new HandlerDescriptor() {
        Route = route,
        Method = "Get",
        Handler = (request) => {
          var requestInjector = new DynamicInjector() {
            { typeof(HttpRequestMessage), request }
          };
          var scope = componentRef.CreateScope(requestInjector);
          return componentRef.InvokeWithProviderFactory(scope, method);
        },
      };
    }
  }

  private class HandlerDescriptor {
    public string Route { get; set; }
    public string Method { get; set; }
    public Func<HttpRequestMessage, object> Handler { get; set; }
  }
}
using System.Reflection;
using Castle.Core.Internal;
using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Generated;
using Mumei.DependencyInjection.Playground.Lib.Http;

namespace Mumei.DependencyInjection.Playground.Example;

public class Program {
  public static void Main() {
    var appEnvironment = PlatformInjector.CreateEnvironment<IApplicationModule>();

    ScanModules(appEnvironment.Instance);

    var common = new CommonModule();
    var weather = new λMumeiλWeatherModule(null, common);
    var app = new ApplicationModuleImpl(weather, common);
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

      var handlerDescriptor = new HandlerDescriptor {
        Route = route,
        Method = "Get",
        Handler = request => {
          var scope = componentRef.CreateScope();

          var requestInjector = StaticInjector.Create(
            scope,
            c => c.Add(request)
          );

          return componentRef.InvokeWithProviderFactory(requestInjector, method)!;
        }
      };
    }
  }

  private sealed class HandlerDescriptor {
    public string Route { get; set; } = default!;
    public string Method { get; set; } = default!;
    public Func<HttpRequestMessage, object> Handler { get; set; } = default!;
  }
}
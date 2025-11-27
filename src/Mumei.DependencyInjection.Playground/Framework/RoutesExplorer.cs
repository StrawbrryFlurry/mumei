using System.Reflection;
using Mumei.DependencyInjection.Injector.Implementation;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Playground.Framework.Http;

namespace Mumei.DependencyInjection.Playground.Framework;

public class RoutesExplorer {
    private static void ScanModules(IModuleRef<IModule> module) {
        foreach (var moduleRef in module.Imports) {
            foreach (var componentRef in moduleRef.Components) {
                var controllerAttribute = componentRef.Type.GetCustomAttribute<ControllerAttribute>();

                if (controllerAttribute is not null) {
                    MapController(componentRef);
                }
            }
        }
    }

    private static void MapController(IComponentRef<IComponent> componentRef) {
        var route = componentRef.Type.GetCustomAttribute<RouteAttribute>()!.Path;
        var methods = componentRef.Type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods) {
            var getAttribute = method.GetCustomAttribute<HttpGetAttribute>();

            if (getAttribute is null) {
                continue;
            }

            var handlerDescriptor = new HandlerDescriptor {
                Route = route,
                Method = "Get",
                Handler = request => {
                    var requestInjector = StaticInjector.Create(
                        componentRef,
                        c => c.Add(request)
                    );

                    var instance = componentRef.Get(componentRef.Type, requestInjector);
                    return method.Invoke(instance, null)!;
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
using Mumei.AspNetCore.Mvc.Roslyn;
using Mumei.DependencyInjection.Injector;

namespace Mumei.AspNetCore.Example.Generated.Mvc;

internal class AppModuleλControllerFactory : MumeiControllerFactory {
  private readonly IInjector _injector;

  public AppModuleλControllerFactory(IInjector injector) {
    _injector = injector;
  }
}
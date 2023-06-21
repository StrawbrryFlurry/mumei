using CleanArchitectureApplication.Application;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated.Application;

public static class λAppModuleλRealizer {
  public static IModuleRef<IAppModule> Get(IInjector parent) {
    var appModule = new λAppModuleRef(parent);
    var applicationModule = RealizeApplicationModule(appModule);

    appModule.Realize(applicationModule);

    return appModule;
  }

  private static IModuleRef<IApplicationModule> RealizeApplicationModule(IInjector parent) {
    var applicationModuleRef = new λApplicationModuleRef(parent);

    var orderingComponentRef = new λOrderingComponentRef(applicationModuleRef);
    orderingComponentRef.Realize();

    applicationModuleRef.Realize(orderingComponentRef);

    return applicationModuleRef;
  }
}
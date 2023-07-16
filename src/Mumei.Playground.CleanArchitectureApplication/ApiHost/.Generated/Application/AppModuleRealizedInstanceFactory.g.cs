using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Persistence;
using CleanArchitectureApplication.Presentation;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Module;

namespace CleanArchitectureApplication.ApiHost.Generated.Application;

public static class λAppModuleRealizer {
  public static λAppModuleRef Realize(IInjector parent) {
    var appModule = new λAppModuleRef(parent);
    var applicationModule = RealizeApplicationModule(appModule);
    var presentationModule = RealizePresentationModule(appModule);
    var persistenceModule = RealizePersistenceModule(appModule);

    var orderingComponentRef = new λOrderingComponentRef(appModule);
    orderingComponentRef.Realize();
    
    appModule.Realize(
      applicationModule,
      presentationModule,
      persistenceModule,
      orderingComponentRef
    );

    return appModule;
  }

  private static IModuleRef<IApplicationModule> RealizeApplicationModule(IInjector parent) {
    var applicationModuleRef = new λApplicationModuleRef(parent);

    var orderingComponentRef = new λOrderingComponentRef(applicationModuleRef);
    orderingComponentRef.Realize();

    applicationModuleRef.Realize(orderingComponentRef);

    return applicationModuleRef;
  }

  private static IModuleRef<IPresentationModule> RealizePresentationModule(IInjector parent) {
    var presentationModuleRef = new λPresentationModuleRef(parent);

    var orderingComponentRef = new λOrderingComponentRef(presentationModuleRef);
    orderingComponentRef.Realize();

    presentationModuleRef.Realize(orderingComponentRef);

    return presentationModuleRef;
  }

  private static IModuleRef<IPersistenceModule> RealizePersistenceModule(IInjector parent) {
    var presentationModuleRef = new λPersistenceModuleRef(parent);
    
    var orderingComponentRef = new λOrderingComponentRef(presentationModuleRef);
    orderingComponentRef.Realize();
    
    presentationModuleRef.Realize(orderingComponentRef);

    return presentationModuleRef;
  }
}
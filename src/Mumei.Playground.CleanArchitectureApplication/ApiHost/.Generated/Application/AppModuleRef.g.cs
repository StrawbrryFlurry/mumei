using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Persistence;
using CleanArchitectureApplication.Presentation;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Module;

namespace CleanArchitectureApplication.ApiHost.Generated.Application;

public sealed class λAppModuleRef : ModuleRef<IAppModule> {
  private IComponentRef<IComponent>[] _components = new IComponentRef<IComponent>[1];
  private IModuleRef<IModule>[] _imports = new IModuleRef<IModule>[5];

  public override IReadOnlyCollection<IComponentRef<IComponent>> Components => _components;
  public override IReadOnlyCollection<IModuleRef<IModule>> Imports => _imports;

  public λAppModuleRef(IInjector parent) : base(parent) { }

  public void Realize(
    IModuleRef<IApplicationModule> applicationModuleRef,
    IModuleRef<IPresentationModule> presentationModuleRef,
    IModuleRef<IPersistenceModule> persistenceModuleRef,
    IComponentRef<IOrderingComponentComposite> orderComponentRef
  ) {
    Injector = new λAppModuleInjector(
      Parent,
      (λApplicationModuleInjector)applicationModuleRef.Injector,
      (λPresentationModuleInjector)presentationModuleRef.Injector,
      (λPersistenceModuleInjector)persistenceModuleRef.Injector,
      (λOrderingComponentInjector)orderComponentRef.Injector
    );

    _imports[0] = applicationModuleRef;
    _imports[1] = presentationModuleRef;
    _imports[2] = persistenceModuleRef;
    
    _components[0] = orderComponentRef;
    
  }
}
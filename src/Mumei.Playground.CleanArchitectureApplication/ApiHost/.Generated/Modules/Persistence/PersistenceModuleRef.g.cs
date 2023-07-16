using Castle.Components.DictionaryAdapter.Xml;
using CleanArchitectureApplication.Persistence;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Module;

namespace CleanArchitectureApplication.ApiHost.Generated; 

public sealed class λPersistenceModuleRef : ModuleRef<IPersistenceModule> {
  private readonly IComponentRef<IComponent>[] _components = new IComponentRef<IComponent>[1];
  public override IReadOnlyCollection<IComponentRef<IComponent>> Components { get; }
  
  private readonly IModuleRef<IModule>[] _imports = new IModuleRef<IModule>[0];
  public override IReadOnlyCollection<IModuleRef<IModule>> Imports { get; }
 
  public λPersistenceModuleRef(IInjector parent) : base(parent) { }

  public void Realize(IComponentRef<IOrderingComponentComposite> orderingComponent) {
    Injector = new λPersistenceModuleInjector(
      Parent,
      (λOrderingComponentInjector)orderingComponent.Injector
    );
    
    _components[0] = orderingComponent;
  }
}
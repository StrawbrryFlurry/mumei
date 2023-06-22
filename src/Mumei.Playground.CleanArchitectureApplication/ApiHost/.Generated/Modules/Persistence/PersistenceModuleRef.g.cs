using Castle.Components.DictionaryAdapter.Xml;
using CleanArchitectureApplication.Persistence;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated; 

public sealed class λPersistenceModuleRef : ModuleRef<IPersistenceModule> {
  public override IReadOnlyCollection<IComponentRef<IComponent>> Components { get; }
  public override IReadOnlyCollection<IModuleRef<IModule>> Imports { get; }
 
  public λPersistenceModuleRef(IInjector parent) : base(parent) { }

  public void Realize() {
    Injector = new λPersistenceModuleInjector(Parent);
  }
}
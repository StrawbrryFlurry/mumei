using CleanArchitectureApplication.Presentation;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Module;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λPresentationModuleRef : ModuleRef<IPresentationModule> {
  private IComponentRef<IComponent>[] _components = new IComponentRef<IComponent>[1];
  private IModuleRef<IModule>[] _imports = new IModuleRef<IModule>[1];

  public override IReadOnlyCollection<IComponentRef<IComponent>> Components => _components;
  public override IReadOnlyCollection<IModuleRef<IModule>> Imports => _imports;

  public λPresentationModuleRef(IInjector parent) : base(parent) { }

  public void Realize(IComponentRef<IOrderingComponentComposite> orderingComponent) {
    Injector = new λPresentationModuleInjector(Parent, (λOrderingComponentInjector)orderingComponent.Injector);

    _components[0] = orderingComponent;
  }
}
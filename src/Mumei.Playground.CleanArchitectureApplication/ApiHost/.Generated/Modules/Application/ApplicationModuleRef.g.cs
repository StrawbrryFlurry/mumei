using CleanArchitectureApplication.ApiHost.Generated;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Module;

namespace CleanArchitectureApplication.Application;

public sealed class λApplicationModuleRef : ModuleRef<IApplicationModule> {
  private readonly IComponentRef<IComponent>[] _components = new IComponentRef<IComponent>[1];
  private readonly IModuleRef<IModule>[] _imports = new IModuleRef<IModule>[0];

  public override IReadOnlyCollection<IComponentRef<IComponent>> Components => _components;
  public override IReadOnlyCollection<IModuleRef<IModule>> Imports => _imports;

  public λApplicationModuleRef(IInjector parent) : base(parent) { }

  public void Realize(IComponentRef<IOrderingComponentComposite> orderingComponent) {
    Injector = new λApplicationModuleInjector(Parent, (λOrderingComponentInjector)orderingComponent.Injector);
    _components[0] = orderingComponent;
  }
}
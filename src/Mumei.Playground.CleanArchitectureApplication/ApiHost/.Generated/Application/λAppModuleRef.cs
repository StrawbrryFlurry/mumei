﻿using CleanArchitectureApplication.Application;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated.Application;

public sealed class λAppModuleRef : ModuleRef<IAppModule> {
  private IComponentRef<IComponent>[] _components = new IComponentRef<IComponent>[0];
  private IModuleRef<IModule>[] _imports = new IModuleRef<IModule>[1];

  public override IReadOnlyCollection<IComponentRef<IComponent>> Components => _components;
  public override IReadOnlyCollection<IModuleRef<IModule>> Imports => _imports;

  public λAppModuleRef(IInjector parent) : base(parent) { }

  public void Realize(IModuleRef<IApplicationModule> applicationModuleRef) {
    Injector = new λAppModuleInjector((λApplicationModuleInjector)applicationModuleRef.Injector);
    _imports[0] = applicationModuleRef;
  }
}
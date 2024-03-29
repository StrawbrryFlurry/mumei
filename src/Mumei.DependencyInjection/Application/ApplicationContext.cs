﻿using Mumei.DependencyInjection.Module;

namespace Mumei.DependencyInjection.Application;

public abstract class ApplicationContext<TModuleImpl> where TModuleImpl : IModule {
  /// <summary>
  ///   Allows users to write custom bootstrapping code
  ///   that is being generated for every component.
  /// </summary>
  public virtual void ComponentBootstrapper(IComponentRef<IComponent> componentRef) { }

  public virtual void ModuleBootstrapper(IModuleRef<IModule> moduleRef) { }
}
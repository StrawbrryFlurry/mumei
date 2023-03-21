namespace Mumei.DependencyInjection.Core;

public abstract class ApplicationContext<TModuleImpl> where TModuleImpl : IModule {
  /// <summary>
  ///   Allows users to write custom bootstrapping code
  ///   that is being generated for every component.
  /// </summary>
  public virtual void ComponentBootstrapper(IComponentRef componentRef) { }

  public virtual void ModuleBootstrapper(IModuleRef moduleRef) { }
}
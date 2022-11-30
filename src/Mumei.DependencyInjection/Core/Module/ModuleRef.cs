namespace Mumei.Core; 

public interface IModuleRef : IModuleRef<IModule> {}

/// <summary>
/// Wraps a runtime instance of <see cref="IModule"/> and provides an interface for interacting with it's members. 
/// </summary>
/// <typeparam name="TModule"></typeparam>
public interface IModuleRef<out TModule> : IInjector where TModule : IModule {
  public Type Type { get; }
  public IModule Injector { get; }
  public IReadOnlyCollection<IComponentRef> Components { get; }
  public IReadOnlyCollection<IModuleRef> Imports { get; }
  
  public IComponentFactoryResolver ComponentFactoryResolver { get; }
  
  public Task<IComponentRef<TComponent>> CreateComponent<TComponent>(
    IInjector? ctx = null
    ) where TComponent : IComponent;
}
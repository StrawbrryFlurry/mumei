using Mumei.Core;

namespace Mumei.DependencyInjection.Playground.Example.Generated;

public sealed class EnvironmentInjector<TAppModule> where TAppModule : IModule {
  public EnvironmentInjector() {
  }
  
  public IInjector Parent { get; } = PlatformInjector.Instance;

  public IModuleRef<TAppModule> Instance { get; } 
  
  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>();
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, flags);
  }
}
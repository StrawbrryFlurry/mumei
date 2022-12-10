using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Playground.Example.Generated;

internal sealed class EnvironmentInjector<TAppModule> where TAppModule : IModule {
  public IInjector Parent { get; } = PlatformInjector.Instance;

  public IModuleRef<TAppModule> Instance { get; } = default!;

  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    return Parent.Get<TProvider>();
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    return Parent.Get(token, flags);
  }
}
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Generated;

internal abstract class ApplicationEnvironment<TAppModule> : IInjector {
  public IModuleRef<TAppModule> Instance { get; } = default!;
  public IInjector Parent { get; } = PlatformInjector.Instance;

  public abstract TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, InjectFlags flags = InjectFlags.None);
}
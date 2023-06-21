using Mumei.DependencyInjection.Core;

namespace Mumei.AspNetCore.Example.Generated;

internal abstract class EnvironmentInjector<TAppModule> : IInjector where TAppModule : IModule {
  public IModuleRef<TAppModule> Instance { get; } = default!;
  public IInjector Parent { get; } = PlatformInjector.Instance;

  public abstract TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None);
}
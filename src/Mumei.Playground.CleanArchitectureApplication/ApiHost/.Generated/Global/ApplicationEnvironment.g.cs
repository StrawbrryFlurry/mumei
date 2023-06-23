using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

internal abstract class ApplicationEnvironment<TAppModule> : IInjector where TAppModule : IModule {
  public abstract IModuleRef<TAppModule> Instance { get; }
  public IInjector Parent { get; } = Platform.Instance;

  public abstract TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None);
}
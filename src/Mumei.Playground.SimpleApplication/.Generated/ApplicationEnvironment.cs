using Mumei.DependencyInjection.Core;

namespace WeatherApplication.Generated;

internal abstract class ApplicationEnvironment<TAppModule> : IInjector where TAppModule : IModule {
  public IModuleRef<TAppModule> ModuleRef { get; } = default!;
  public TAppModule Module { get; } = default!;
  public IInjector Parent { get; } = PlatformInjector.Instance;

  public abstract TProvider Get<TProvider>(IInjector? scope, InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, IInjector? scope, InjectFlags flags = InjectFlags.None);
}
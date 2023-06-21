using CleanArchitectureApplication;
using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.ApiHost.Generated.Application;
using Mumei.DependencyInjection.Core;

namespace Mumei.Playground.SimpleApplication;

internal sealed class AppModuleλApplicationEnvironment : ApplicationEnvironment<IAppModule> {
  private IModuleRef<IAppModule>? _instance = null;
  
  public override IModuleRef<IAppModule> Instance => _instance ??= λAppModuleλRealizer.Get(this);
  
  public override TProvider Get<TProvider>(IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    return Instance.Get<TProvider>(scope, flags);
  }

  public override object Get(object token, IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    return Instance.Get(token, scope, flags);
  }
}
using CleanArchitectureApplication;
using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.ApiHost.Generated.Application;
using Mumei.DependencyInjection.Core;

namespace Mumei.Playground.SimpleApplication;

internal sealed class λAppModuleApplicationEnvironment : ApplicationEnvironment<IAppModule> {
  private λAppModuleRef _instance;
  public override IModuleRef<IAppModule> Instance => _instance;

  private λAppModuleInjector Injector => (λAppModuleInjector)_instance.Injector;
  
  public λAppModuleApplicationEnvironment() {
    _instance = λAppModuleRealizer.Realize(this);
  }
  
  public override TProvider Get<TProvider>(IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);    
  }

  public override object Get(object token, IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    if (Injector.TryGet(token, scope, flags, out var instance)) {
      return instance;
    }
    
    return Parent.Get(token, scope, flags);
  }
}
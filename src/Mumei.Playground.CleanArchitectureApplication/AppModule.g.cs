using CleanArchitectureApplication.Infrastructure.Order;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Internal;

namespace CleanArchitectureApplication;

[MumeiGenerated]
[MumeiModuleImplFor<IAppModule>]
public sealed class λAppModule : IAppModule {
  internal readonly λAppModuleλOrderInfrastructureModule λOrderModule;

  public IOrderInfrastructureModule Order => λOrderModule;

  public IInjector Parent { get; }

  public λAppModule(λAppModuleλOrderInfrastructureModule orderModule) {
    λOrderModule = orderModule;
  }
  
  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public IInjector CreateScope() {
    throw new NotImplementedException();
  }

  public IInjector CreateScope(IInjector context) {
    throw new NotImplementedException();
  }
}
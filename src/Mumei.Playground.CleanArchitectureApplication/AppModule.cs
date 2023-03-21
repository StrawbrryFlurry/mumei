using CleanArchitectureApplication.Infrastructure.Order;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication;

[ApplicationRoot]
[Import<IOrderInfrastructureModule>]
public partial interface IAppModule { }

public partial interface IAppModule : IModule {
  public IOrderInfrastructureModule Order { get; }
}
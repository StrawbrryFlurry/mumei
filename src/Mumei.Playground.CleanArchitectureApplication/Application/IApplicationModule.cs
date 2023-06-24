using CleanArchitectureApplication.Application.Ordering;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Application;

public partial interface IApplicationModule : IModule {
  public IOrderComponent Ordering { get; }
}

[Module]
[Component<IOrderComponent>]
[DynamicallyBind<MediatrBinder<IApplicationModule>>]
public partial interface IApplicationModule { }
using CleanArchitectureApplication.Application.Ordering;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Module.Registration;

namespace CleanArchitectureApplication.Application;

public partial interface IApplicationModule : IModule {
  public IOrderComponent Ordering { get; }
}

[Module]
[Component<IOrderComponent>]
[DynamicallyBind<MediatrBinder<IApplicationModule>>]
public partial interface IApplicationModule { }
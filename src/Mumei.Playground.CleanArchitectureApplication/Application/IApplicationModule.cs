using CleanArchitectureApplication.Application.Ordering;
using Mumei.DependencyInjection.Attributes;

namespace CleanArchitectureApplication.Application;

[Module]
[Component<IOrderComponent>]
[DynamicallyBind<MediatrBinder<IApplicationModule>>]
public partial interface IApplicationModule { }
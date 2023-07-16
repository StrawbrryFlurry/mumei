using CleanArchitectureApplication.Domain.Ordering;
using MediatR;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace CleanArchitectureApplication.Presentation.Ordering;

public partial interface IOrderComponent : IComponent {
  OrderController OrderController { get; }
}

[OrderingComponent]
[Scoped<OrderController>]
public partial interface IOrderComponent {
  [ForwardRef(ForwardRefSource.Parent)]
  IMediator Mediator { get; }
}
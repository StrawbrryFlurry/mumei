using CleanArchitectureApplication.Domain.Ordering;
using MediatR;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Presentation.Ordering;

public partial interface IOrderComponent : IComponent { }

[OrderComponent]
[Scoped<OrderController>]
public partial interface IOrderComponent {
  [ForwardRef(ForwardRefSource.Parent)]
  IMediator Mediator { get; }
}
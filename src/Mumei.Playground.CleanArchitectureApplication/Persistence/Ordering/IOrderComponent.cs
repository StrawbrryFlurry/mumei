using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace CleanArchitectureApplication.Persistence.Ordering;

[OrderingComponent]
public partial interface IOrderComponent {
  [Scoped<OrderRepository>]
  public IOrderRepository OrderRepository { get; }
}

public partial interface IOrderComponent : IComponent { }
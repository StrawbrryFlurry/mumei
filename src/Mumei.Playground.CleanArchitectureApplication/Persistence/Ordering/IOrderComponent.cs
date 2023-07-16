using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace CleanArchitectureApplication.Persistence.Ordering;

[OrderingComponent]
public interface IOrderComponent {
  [Scoped<OrderRepository>]
  public IOrderRepository OrderRepository { get; }
}
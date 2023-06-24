using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Attributes;

namespace CleanArchitectureApplication.Persistence.Ordering;

[OrderingComponent]
public interface IOrderComponent {
  [Scoped<OrderRepository>]
  public IOrderRepository OrderRepository { get; }
}
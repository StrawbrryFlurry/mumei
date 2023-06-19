using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Attributes;

namespace CleanArchitectureApplication.Persistence.Ordering;

[OrderComponent]
public interface IOrderComponent {
  [Scoped<OrderRepository>]
  public IOrderRepository OrderRepository { get; }
}
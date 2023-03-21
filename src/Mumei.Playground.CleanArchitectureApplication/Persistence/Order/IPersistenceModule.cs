using CleanArchitectureApplication.Domain.Order;
using Mumei.DependencyInjection.Attributes;

namespace CleanArchitectureApplication.Persistence.Order;

public interface IPersistenceModule {
  [Scoped<OrderRepository>]
  public IOrderRepository OrderRepository { get; }
}
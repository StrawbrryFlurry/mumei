using System.Collections.Immutable;
using CleanArchitectureApplication.Domain.Ordering;

namespace CleanArchitectureApplication.Persistence.Ordering;

internal sealed class OrderRepository : IOrderRepository {
  private List<Order> _orders = new();

  public void Add(Order order) {
    _orders.Add(order);
  }

  public IReadOnlyCollection<Order> GetAll() {
    return _orders.ToImmutableList();
  }
}
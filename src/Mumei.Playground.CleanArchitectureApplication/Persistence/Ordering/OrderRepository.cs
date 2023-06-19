using CleanArchitectureApplication.Domain.Ordering;

namespace CleanArchitectureApplication.Persistence.Ordering;

internal sealed class OrderRepository : IOrderRepository {
  private List<Order> _orders = new();

  public void Add(Order order) {
    _orders.Add(order);
  }
}
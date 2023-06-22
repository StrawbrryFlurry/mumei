namespace CleanArchitectureApplication.Domain.Ordering;

public sealed class Order {
  public Guid Id { get; set; }

  public Guid CustomerId { get; }
  public List<Guid> ProductIds { get; }

  public Order(Guid customerId, List<Guid> productIds) {
    Id = Guid.NewGuid();
    CustomerId = customerId;
    ProductIds = productIds;
  }
}
namespace CleanArchitectureApplication.Domain.Ordering;

public sealed class Order {
  public Guid Id { get; set; }

  public string CustomerId { get; }
  public List<string> ProductIds { get; }

  public Order(string customerId, List<string> productIds) {
    Id = Guid.NewGuid();
    CustomerId = customerId;
    ProductIds = productIds;
  }
}
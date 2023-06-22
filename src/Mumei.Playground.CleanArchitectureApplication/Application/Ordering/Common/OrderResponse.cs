namespace CleanArchitectureApplication.Application.Ordering.Common;

public sealed class OrderResponse {
  public required Guid Id { get; init; }
  public required Guid CustomerId { get; init; }
  public required List<Guid> ProductIds { get; init; }
}
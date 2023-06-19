namespace CleanArchitectureApplication.Application.Ordering.Common;

public sealed class OrderResponse {
  public required Guid Id { get; init; }
  public required string CustomerId { get; init; }
  public required List<string> ProductIds { get; init; }
}
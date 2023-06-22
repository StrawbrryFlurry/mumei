using CleanArchitectureApplication.Application.Ordering.Common;
using MediatR;

namespace CleanArchitectureApplication.Application.Ordering.Commands.CreateOrderCommand;

public sealed record CreateOrderCommand(
  Guid CustomerId,
  List<Guid> ProductIds
) : IRequest<OrderResponse>;
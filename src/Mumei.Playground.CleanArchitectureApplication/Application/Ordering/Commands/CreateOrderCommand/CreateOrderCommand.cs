using CleanArchitectureApplication.Application.Ordering.Common;
using MediatR;

namespace CleanArchitectureApplication.Application.Ordering.Commands.CreateOrderCommand;

public sealed record CreateOrderCommand(
  string CustomerId,
  List<string> ProductIds
) : IRequest<OrderResponse>;
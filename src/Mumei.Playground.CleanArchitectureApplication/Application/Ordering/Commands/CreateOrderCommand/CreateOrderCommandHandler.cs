using CleanArchitectureApplication.Application.Ordering.Common;
using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Domain.Ordering;
using MediatR;

namespace CleanArchitectureApplication.Application.Ordering.Commands.CreateOrderCommand;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse> {
  private readonly IOrderRepository _repository;
  private readonly IUnitOfWork _unitOfWork;

  public CreateOrderCommandHandler(IOrderRepository repository, IUnitOfWork unitOfWork) {
    _repository = repository;
    _unitOfWork = unitOfWork;
  }

  public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken) {
    var order = new Order(request.CustomerId, request.ProductIds);

    _repository.Add(order);
    await _unitOfWork.CommitAsync(cancellationToken);

    return new OrderResponse {
      Id = order.Id,
      CustomerId = order.CustomerId,
      ProductIds = order.ProductIds
    };
  }
}
using CleanArchitectureApplication.Application.Ordering.Commands.CreateOrderCommand;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureApplication.Presentation.Ordering;

public sealed class OrderController : ControllerBase {
  private readonly IMediator _mediator;

  public OrderController(IMediator mediator) {
    _mediator = mediator;
  }

  [HttpPost]
  public async Task<IActionResult> Create(Guid customerId, List<Guid> productIds) {
    var order = await _mediator.Send(new CreateOrderCommand(customerId, productIds));
    return Created($"/{order.Id}", order);
  }
}
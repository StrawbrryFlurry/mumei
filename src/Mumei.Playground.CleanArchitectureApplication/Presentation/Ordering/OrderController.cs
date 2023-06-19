using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureApplication.Presentation.Ordering;

public sealed class OrderController : ControllerBase {
  private readonly IMediator _mediator;

  public OrderController(IMediator mediator) {
    _mediator = mediator;
  }

  [HttpGet]
  public IActionResult Get() {
    return Ok();
  }
}
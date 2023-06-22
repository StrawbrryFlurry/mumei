using CleanArchitectureApplication.Presentation.Ordering;
using MediatR;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class OrderControllerλBinding : ScopedBinding<OrderController> {
  private readonly Binding<IMediator> _mediatrProvider;

  public OrderControllerλBinding(Binding<IMediator> mediatorBinding) {
    _mediatrProvider = mediatorBinding;
  }

  protected override OrderController Create(IInjector? scope = null) {
    return new OrderController(_mediatrProvider.Get(scope));
  }
}
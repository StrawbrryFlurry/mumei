using CleanArchitectureApplication.Presentation.Ordering;
using MediatR;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Resolution;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λOrderControllerBinding : ScopedBinding<OrderController> {
  private readonly Binding<IMediator> _mediatrProvider;

  public λOrderControllerBinding(Binding<IMediator> mediatorBinding) {
    _mediatrProvider = mediatorBinding;
  }

  protected override OrderController Create(IInjector? scope = null) {
    return new OrderController(_mediatrProvider.Get(scope));
  }
}
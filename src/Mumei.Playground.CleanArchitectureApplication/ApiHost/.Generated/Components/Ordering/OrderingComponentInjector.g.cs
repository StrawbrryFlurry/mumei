using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Presentation.Ordering;
using MediatR;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λOrderingComponentInjector : IOrderingComponentComposite {
  public IInjector Parent { get; set; }

  internal readonly Binding<IOrderRepository> OrderRepositoryBinding;
  internal Binding<OrderController> OrderControllerBinding { get; private set; }
  internal Binding<IMediator> MediatorBinding { get; private set; }
  
  public IMediator Mediator => MediatorBinding.Get(this);

  public λOrderingComponentInjector(IInjector parent) {
    Parent = parent;
    MediatorBinding = new LateBoundBinding<IMediator>(Parent, injector => injector.Get<Binding<IMediator>>());
    OrderControllerBinding = new λOrderControllerBinding(MediatorBinding);
    OrderRepositoryBinding = new λOrderRepositoryBinding();
  }

  public IOrderRepository OrderRepository => OrderRepositoryBinding.Get();

  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    scope ??= this;
    if ((flags & InjectFlags.SkipSelf) != 0) {
      return Parent.Get(token, scope, flags & ~InjectFlags.SkipSelf);
    }

    if ((flags & InjectFlags.Host) != 0) {
      Get<IModuleRef<IModule>>().Get(token, scope, flags & ~InjectFlags.Host);
    }

    var result = token switch {
      _ when ReferenceEquals(token, typeof(IOrderRepository)) => OrderRepositoryBinding.Get(scope),
      _ when ReferenceEquals(token, typeof(OrderController)) => OrderControllerBinding.Get(scope),
      _ => Parent.Get(token, scope, flags)
    };

    return result;
  }

}
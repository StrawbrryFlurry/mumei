using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Presentation.Ordering;
using MediatR;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λOrderingComponentInjector : IOrderingComponentComposite {
  public IInjector Parent { get; set; }
  public InjectorBloomFilter Bloom => new λOrderingComponentInjectorBloom();
  
  private readonly Binding<IMediator> _mediatorBinding;
  public IMediator Mediator => _mediatorBinding.Get(this);
  
  private readonly Binding<OrderController> _orderControllerBinding;
  public OrderController OrderController => _orderControllerBinding.Get(this);
  
  private readonly Binding<IOrderRepository> _orderRepositoryBinding;
  public IOrderRepository OrderRepository => _orderRepositoryBinding.Get(this);

  public λOrderingComponentInjector(IInjector parent) {
    Parent = parent;
    _mediatorBinding = new LateBoundBinding<IMediator>(scope => this.Get<Binding<IMediator>>(scope));
    _orderControllerBinding = new λOrderControllerBinding(_mediatorBinding);
    _orderRepositoryBinding = new λOrderRepositoryBinding();
  }

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

    if (TryGet(token, scope, flags, out var result)) {
      return result;
    }
    
    return Parent.Get(token, scope, flags);
  }

  public bool TryGet(object token, IInjector scope, InjectFlags flags, out object? instance) {
    if (!Bloom.Contains(token)) {
      instance = null;
      return false;
    }
    
    if (ReferenceEquals(token, typeof(IOrderRepository))) {
      instance = _orderRepositoryBinding.Get(scope);
      return true;
    }

    if (ReferenceEquals(token, typeof(OrderController))) {
      instance = _orderControllerBinding.Get(scope);
      return true;
    }
    
    if (ReferenceEquals(token, typeof(IMediator))) {
      instance = _mediatorBinding.Get(scope);
      return true;
    }

    instance = null!;
    return false;
  }
 
  internal sealed class λOrderingComponentInjectorBloom : InjectorBloomFilter {
    public λOrderingComponentInjectorBloom() {
      Add(typeof(OrderController));
      Add(typeof(IOrderRepository));
      Add(typeof(IMediator));
    }
  }
}
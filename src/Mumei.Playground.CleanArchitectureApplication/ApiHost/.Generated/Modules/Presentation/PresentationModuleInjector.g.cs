using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Presentation;
using CleanArchitectureApplication.Presentation.Ordering;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λPresentationModuleInjector : IPresentationModule {
  public IInjector Parent { get; }
  
  internal readonly λOrderingComponentInjector λOrderingComponent;

  public λPresentationModuleInjector(IInjector parent, λOrderingComponentInjector orderingComponent) {
    λOrderingComponent = orderingComponent;
    Parent = parent;
  }
  
  public TProvider Get<TProvider>(IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    scope ??= this;
    if ((flags & InjectFlags.SkipSelf) != 0) {
      return Parent.Get(token, scope, flags & ~InjectFlags.SkipSelf);   
    }

    object? instance = null;
    if (TryGet(token, scope, flags, out instance)) {
      return instance;
    }
    
    return Parent.Get(token, scope, flags);
  }
  
  internal bool TryGet(object token, IInjector scope, InjectFlags flags, out object? instance) {
    if (TryGetOrderingComponentProvider(token, scope, flags, out instance)) {
      return true;
    }

    instance = null!;
    return false;
  }
  
  private bool TryGetOrderingComponentProvider(object token, IInjector scope, InjectFlags flags, out object? instance) {
    if (token == typeof(OrderController)) {
      instance = λOrderingComponent.OrderControllerBinding.Get(scope);
      return true;
    }
    
    if (token == typeof(IOrderRepository)) {
      instance = λOrderingComponent.OrderRepositoryBinding.Get(scope);
      return true;
    }

    instance = null;
    return false;
  }
}
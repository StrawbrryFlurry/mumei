using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Presentation;
using CleanArchitectureApplication.Presentation.Ordering;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;
using Mumei.DependencyInjection.Injector.Resolution;
using IOrderComponent = CleanArchitectureApplication.Presentation.Ordering.IOrderComponent;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λPresentationModuleInjector : IPresentationModule {
  public readonly InjectorBloomFilter Bloom;
  public IInjector Parent { get; }
  
  internal readonly λOrderingComponentInjector _orderingComponent;
  public IOrderComponent Ordering => _orderingComponent;

  public λPresentationModuleInjector(IInjector parent, λOrderingComponentInjector orderingComponent) {
    Parent = parent;
    _orderingComponent = orderingComponent;
    
    Bloom = new λPresentationModuleBloom(_orderingComponent);
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
    if (!Bloom.Contains(token)) {
      instance = null!;
      return false;
    }
    
    if (_orderingComponent.TryGet(token, scope, flags, out instance)) {
      return true;
    }

    instance = null!;
    return false;
  }
  
  internal sealed class λPresentationModuleBloom : InjectorBloomFilter {
    public λPresentationModuleBloom(
      λOrderingComponentInjector orderingComponent
    ) {
      Merge(orderingComponent.Bloom);
    }
  }
}
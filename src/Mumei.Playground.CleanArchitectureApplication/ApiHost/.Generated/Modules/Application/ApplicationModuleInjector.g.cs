using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Core;
using IOrderComponent = CleanArchitectureApplication.Application.Ordering.IOrderComponent;

namespace CleanArchitectureApplication.Application;

public partial interface IApplicationModule : IModule {
  public IOrderComponent Ordering { get; }  
}

public sealed class λApplicationModuleInjector : IApplicationModule {
  public IInjector Parent { get; }

  internal readonly DynamicProviderBinder λMediatrBinderλIApplicationModule;
  internal readonly λApplicationModuleλBloom λBloom = new();
  
  internal readonly λOrderingComponentInjector λOrderingComponent;
  
  public IOrderComponent Ordering => λOrderingComponent;
  
  public λApplicationModuleInjector(IInjector parent, λOrderingComponentInjector orderingComponent) {
    Parent = parent;
    λOrderingComponent = orderingComponent;
    λMediatrBinderλIApplicationModule = new ApplicationModuleλMediatrBinderλIApplicationModule(this);
  }
  
  public TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None) {
    scope ??= this;
    if ((flags & InjectFlags.SkipSelf) != 0) {
      return Parent.Get(token, scope, flags & ~InjectFlags.SkipSelf);   
    }
    
    // Don't check for Host because this is a module host.
    
    object? instance = null;
    if (TryGet(token, scope, flags, out instance)) {
      return instance;
    }
    
    return Parent.Get(token, scope, flags);
  }
  
  internal bool TryGet(object token, IInjector scope, InjectFlags flags, out object? instance) {
    if (λMediatrBinderλIApplicationModule.TryGet(token, out instance)) {
      return true;
    }
    
    if (TryGetOrderingComponentProvider(token, scope, flags, out instance)) {
      return true;
    }
    
    instance = null!;
    return false;
  }

  private bool TryGetOrderingComponentProvider(object token, IInjector scope, InjectFlags flags, out object? instance) {
    if (token == typeof(IOrderComponent)) {
      instance = λOrderingComponent;
      return true;
    }
    
    if (token == typeof(IOrderRepository)) {
      instance = λOrderingComponent.OrderRepositoryBinding.Get(scope);
      return true;
    }
    
    instance = null!;
    return false;
  }

  internal sealed class λApplicationModuleλBloom {
    public bool MightContainProvider(object token) {
      return true;
    }
  }
}
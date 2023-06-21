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

  private readonly DynamicProviderBinder λMediatrBinderλIApplicationModule;
  
  private readonly ProviderCollection _dynamicProviders = new();
  
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
    
    if (λMediatrBinderλIApplicationModule.TryGet(token, out var provider)) {
      return provider;
    }
    
    return token switch {
      _ when ReferenceEquals(token, typeof(IOrderComponent)) => λOrderingComponent,
      _ when ReferenceEquals(token, typeof(IOrderRepository)) => λOrderingComponent.OrderRepositoryBinding.Get(scope),
      _ => Parent.Get(token)
    };
  }
  
  public IInjector CreateScope() {
    throw new NotImplementedException();
  }

  public IInjector CreateScope(IInjector context) {
    throw new NotImplementedException();
  }

  internal sealed class λApplicationModuleλBloom {
    public bool MightContainProvider(object token) {
      return true;
    }
  }
}
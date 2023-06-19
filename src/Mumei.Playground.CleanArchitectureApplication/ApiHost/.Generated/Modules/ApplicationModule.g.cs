using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Core;
using IOrderComponent = CleanArchitectureApplication.Application.Ordering.IOrderComponent;

namespace CleanArchitectureApplication.Application;

public partial interface IApplicationModule : IModule {
  public IOrderComponent Ordering { get; }  
}

public sealed class λApplicationModule : IApplicationModule {
  public IInjector Scope { get; }
  public IInjector Parent { get; }

  private readonly ProviderCollection _dynamicProviders = new();
  internal readonly MediatrBinder<IApplicationModule> λMediatrBinder;
  
  internal readonly λOrderingComponent λOrderingComponent;
  
  public IOrderComponent Ordering => λOrderingComponent;

  public λApplicationModule(IInjector scope, λOrderingComponent orderingComponent) {
    Scope = scope;
    λOrderingComponent = orderingComponent;
  }
  
  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    var token = typeof(TProvider);
    var result = token switch {
      _ when ReferenceEquals(token, typeof(IOrderComponent)) => λOrderingComponent,
      _ => Parent.Get(token)
    };
    
    return (TProvider)result;
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    return token switch {
      _ when ReferenceEquals(token, typeof(IOrderComponent)) => λOrderingComponent,
      _ when ReferenceEquals(token, typeof(IOrderRepository)) => λOrderingComponent.OrderRepositoryBinding.Get(Scope),
      _ => Parent.Get(token)
    };
  }
  
  public IInjector CreateScope() {
    throw new NotImplementedException();
  }

  public IInjector CreateScope(IInjector context) {
    throw new NotImplementedException();
  }
}
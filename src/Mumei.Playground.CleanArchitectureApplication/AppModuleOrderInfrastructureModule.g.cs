using CleanArchitectureApplication.Domain.Order;
using CleanArchitectureApplication.Infrastructure.Order;
using CleanArchitectureApplication.Persistence.Order;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication; 

public class λAppModuleλOrderInfrastructureModule : IOrderInfrastructureModule {
  internal readonly Binding<IOrderRepository> λOrderRepositoryBinding;

  public IOrderRepository OrderRepository => λOrderRepositoryBinding.Get();

  public λAppModuleλOrderInfrastructureModule() {
    λOrderRepositoryBinding = new OrderRepositoryBinding();
  }

  public IInjector Parent { get; }
  
  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public IInjector CreateScope() {
    throw new NotImplementedException();
  }

  public IInjector CreateScope(IInjector context) {
    throw new NotImplementedException();
  }
}

file class OrderRepositoryBinding : ScopedBinding<IOrderRepository> {
  protected override IOrderRepository Create(IInjector scope = null) {
    return new OrderRepository();
  }
} 
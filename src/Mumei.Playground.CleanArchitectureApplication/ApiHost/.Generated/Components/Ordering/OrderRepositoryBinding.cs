using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Persistence.Ordering;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public class OrderRepositoryλBinding : ScopedBinding<IOrderRepository> {
  protected override IOrderRepository Create(IInjector? scope = null) {
    return new OrderRepository();
  }
}
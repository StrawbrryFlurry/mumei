using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Persistence.Ordering;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λOrderRepositoryBinding : ScopedBinding<IOrderRepository> {
  protected override IOrderRepository Create(IInjector? scope = null) {
    return new OrderRepository();
  }
}
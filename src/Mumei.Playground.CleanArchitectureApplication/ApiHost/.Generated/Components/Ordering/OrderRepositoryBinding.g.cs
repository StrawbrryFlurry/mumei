using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Persistence.Ordering;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Resolution;

namespace CleanArchitectureApplication.ApiHost.Generated;

public sealed class λOrderRepositoryBinding : ScopedBinding<IOrderRepository> {
  protected override IOrderRepository Create(IInjector? scope = null) {
    return new OrderRepository();
  }
}
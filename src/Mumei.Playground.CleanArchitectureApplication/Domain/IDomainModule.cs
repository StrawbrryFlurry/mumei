using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace CleanArchitectureApplication.Domain;

[Contributes<IOrderComponent>]
public partial interface IDomainModule {
  [ForwardRef(ForwardRefSource.Parent)]
  public IUnitOfWork UnitOfWork { get; }
}

public partial interface IDomainModule : IModule {
  public IOrderComponent OrderComponent { get; }
}
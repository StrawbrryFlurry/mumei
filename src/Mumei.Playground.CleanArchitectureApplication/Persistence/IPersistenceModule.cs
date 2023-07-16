using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Persistence.Common;
using CleanArchitectureApplication.Persistence.Ordering;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace CleanArchitectureApplication.Persistence;

public partial interface IPersistenceModule : IModule {
  public IOrderComponent Ordering { get; }
}

[Import<IOrderComponent>]
public partial interface IPersistenceModule {
  [Scoped<UnitOfWork>]
  public IUnitOfWork UnitOfWork { get; }

  [ForwardRef(ForwardRefSource.Parent)]
  public PersistenceOptions Options { get; }
}
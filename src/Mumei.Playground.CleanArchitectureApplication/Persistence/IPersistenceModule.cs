using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Persistence.Common;
using CleanArchitectureApplication.Persistence.Ordering;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Persistence;

public partial interface IPersistenceModule : IModule { }

[Import<IOrderComponent>]
public partial interface IPersistenceModule {
  [Scoped<UnitOfWork>]
  public IUnitOfWork UnitOfWork { get; }

  [ForwardRef(ForwardRefSource.Parent)]
  public PersistenceOptions Options { get; }
}
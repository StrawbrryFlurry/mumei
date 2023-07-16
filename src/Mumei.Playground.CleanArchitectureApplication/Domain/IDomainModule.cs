using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;

namespace CleanArchitectureApplication.Domain;

[Import<IOrderComponent>]
public interface IDomainModule {
  [ForwardRef(ForwardRefSource.Parent)]
  public IUnitOfWork UnitOfWork { get; }
}
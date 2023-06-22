using CleanArchitectureApplication.Domain.Common.Persistence;
using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Domain;

[Import<IOrderComponent>]
public interface IDomainModule {
  [ForwardRef(ForwardRefSource.Parent)]
  public IUnitOfWork UnitOfWork { get; }
}
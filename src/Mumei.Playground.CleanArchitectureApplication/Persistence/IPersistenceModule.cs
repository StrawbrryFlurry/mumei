using CleanArchitectureApplication.Persistence.Ordering;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Persistence;

[Import<IOrderComponent>]
public interface IPersistenceModule {
  [ForwardRef(ForwardRefSource.Parent)]
  public PersistenceOptions Options { get; }
}
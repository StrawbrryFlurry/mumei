using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Domain.Ordering;

public partial interface IOrderComponent {
  [ForwardRef(ForwardRefSource.Sibling)]
  public IOrderRepository OrderRepository { get; }
}
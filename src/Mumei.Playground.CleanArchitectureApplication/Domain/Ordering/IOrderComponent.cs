using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Domain.Ordering;

public partial interface IOrderComponent {
  [ForwardRef]
  public IOrderRepository OrderRepository { get; }
}
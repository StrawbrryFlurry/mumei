using Mumei.DependencyInjection.Providers.Registration;

namespace CleanArchitectureApplication.Domain.Ordering;

public partial interface IOrderComponent {
  [ForwardRef]
  public IOrderRepository OrderRepository { get; }
}
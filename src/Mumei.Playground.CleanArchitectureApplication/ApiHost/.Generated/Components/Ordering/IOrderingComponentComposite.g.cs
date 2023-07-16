using CleanArchitectureApplication.Domain.Ordering;

namespace CleanArchitectureApplication.ApiHost.Generated;

public interface IOrderingComponentComposite :
  IOrderComponent,
  CleanArchitectureApplication.Application.Ordering.IOrderComponent,
  Persistence.Ordering.IOrderComponent,
  Presentation.Ordering.IOrderComponent {
  public IOrderRepository OrderRepository { get; }
}
using CleanArchitectureApplication.Domain.Ordering;

namespace CleanArchitectureApplication.ApiHost.Generated;

public interface IOrderingComponentComposite :
  IOrderComponent,
  Application.Ordering.IOrderComponent,
  Persistence.Ordering.IOrderComponent,
  Presentation.Ordering.IOrderComponent {
  public IOrderRepository OrderRepository { get; }
}
using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.ApiHost.Generated;

public interface IOrderingComponentComposite :
  IOrderComponent,
  CleanArchitectureApplication.Application.Ordering.IOrderComponent,
  Persistence.Ordering.IOrderComponent,
  Presentation.Ordering.IOrderComponent,
  IComponent {
  public IOrderRepository OrderRepository { get; }
}
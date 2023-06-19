using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Attributes;

namespace CleanArchitectureApplication.Presentation.Ordering;

[OrderComponent]
[Scoped<OrderController>]
public interface IOrderComponent { }
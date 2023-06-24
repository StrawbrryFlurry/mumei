using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Application.Ordering;

[OrderingComponent]
public partial interface IOrderComponent : IComponent { }

[Component("Ordering")]
public partial interface IOrderComponent { }
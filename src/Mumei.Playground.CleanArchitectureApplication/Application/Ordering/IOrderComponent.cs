using CleanArchitectureApplication.Domain.Ordering;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Application.Ordering;

public partial interface IOrderComponent : IComponent { }

[OrderComponent]
public partial interface IOrderComponent { }
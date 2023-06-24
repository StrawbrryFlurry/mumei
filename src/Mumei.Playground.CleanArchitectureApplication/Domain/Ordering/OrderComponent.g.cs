using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Domain.Ordering; 

public sealed class OrderingComponentAttribute : Attribute, IComponentMarker { }

[OrderingComponent]
public partial interface IOrderComponent : IComponent { }
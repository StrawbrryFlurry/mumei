using Mumei.DependencyInjection.Module;

namespace CleanArchitectureApplication.Domain.Ordering; 

public sealed class OrderingComponentAttribute : ComponentMarkerAttribute { }

[OrderingComponent]
public partial interface IOrderComponent : IComponent { }
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Domain.Ordering; 

public sealed class OrderComponentAttribute : Attribute, IComponentMarker { }

[OrderComponent]
public partial interface IOrderComponent { }
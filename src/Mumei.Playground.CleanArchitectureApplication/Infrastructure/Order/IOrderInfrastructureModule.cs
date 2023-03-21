using CleanArchitectureApplication.Application.Order;
using CleanArchitectureApplication.Persistence.Order;
using CleanArchitectureApplication.Presentation.Order;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace CleanArchitectureApplication.Infrastructure.Order;

[Module]
public partial interface IOrderInfrastructureModule :
  IOrderApplicationModule,
  IOrderPresentationModule,
  IPersistenceModule { }

public partial interface IOrderInfrastructureModule : IModule { }
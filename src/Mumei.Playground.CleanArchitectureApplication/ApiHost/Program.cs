using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Presentation.Ordering;
using Mumei.DependencyInjection.Core;

var appEnvironment = Platform.CreateEnvironment<IAppModule>();

var orderController = appEnvironment.Get<OrderController>(SingletonScopeλInjector.Instance);

var orderRepository = appEnvironment.Get<IOrderRepository>(SingletonScopeλInjector.Instance);

var order = await orderController.Create(Guid.NewGuid(), new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() });

var orders = orderRepository.GetAll();

Console.Read();
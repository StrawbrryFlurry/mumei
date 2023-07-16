using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Application;
using CleanArchitectureApplication.Domain.Ordering;
using CleanArchitectureApplication.Presentation.Ordering;

var appEnvironment = Platform.CreateEnvironment<IAppModule>();

var orderController = appEnvironment.Get<OrderController>();

var orderRepository = appEnvironment.Get<IOrderRepository>();

var order = await orderController.Create(Guid.NewGuid(), new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() });

var orders = orderRepository.GetAll();

Console.Read();
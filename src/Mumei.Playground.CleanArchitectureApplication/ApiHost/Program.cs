using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Presentation.Ordering;

var appEnvironment = Platform.CreateEnvironment<IAppModule>();

var orderController = appEnvironment.Get<OrderController>();

var order = await orderController.Create(Guid.NewGuid(), new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() });

Console.Read();
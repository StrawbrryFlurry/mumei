using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Presentation.Ordering;

var app = PlatformInjector.CreateEnvironment<IAppModule>();

var orderController = app.Get<OrderController>();

var order = await orderController.Create(Guid.NewGuid(), new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() });

Console.Read();
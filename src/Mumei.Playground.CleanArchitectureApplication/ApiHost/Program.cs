using CleanArchitectureApplication.ApiHost;
using CleanArchitectureApplication.ApiHost.Generated;
using CleanArchitectureApplication.Domain.Ordering;

var app = PlatformInjector.CreateEnvironment<IAppModule>();

var r = app.Get<IOrderRepository>();
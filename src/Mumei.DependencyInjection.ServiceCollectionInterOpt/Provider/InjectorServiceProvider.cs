using Microsoft.Extensions.DependencyInjection;
using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.ServiceCollectionInterOpt.Provider;

public sealed class InjectorServiceProvider : IServiceProvider {
  private readonly IInjector _injector;
  private readonly IServiceProvider _serviceProvider;

  public InjectorServiceProvider(IInjector injector) {
    _injector = injector;
    var serviceCollection = (IServiceCollection?)injector.Get<IServiceCollection>(null, InjectFlags.Optional) ??
                            new ServiceCollection();
    _serviceProvider = serviceCollection.BuildServiceProvider();
  }

  public object? GetService(Type serviceType) {
    try {
      var service = _serviceProvider.GetService(serviceType);
      if (service is not null) {
        return service;
      }
    }
    catch {
      // The service provider may throw an exception if the service type is not registered.
    }

    return _injector.Get(serviceType);
  }
}
using Mumei.DependencyInjection.Core;

namespace Mumei.AspNetCore;

internal sealed class ServiceProviderAdapter : IServiceProvider {
  private readonly IInjector _injector;

  public ServiceProviderAdapter(IServiceCollection collection, IInjector injector) {
    _injector = injector;
  }

  public object? GetService(Type serviceType) {
    return _injector.Get(serviceType);
  }
}
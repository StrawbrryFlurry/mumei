using Microsoft.Extensions.DependencyInjection;
using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.ServiceCollectionInterOpt.Provider;

public static class ProviderServiceCollectionExtensions {
  public static ProviderCollection AddFromServiceCollection(
    this ProviderCollection providers,
    Action<IServiceCollection> serviceCollectionConfiguration
  ) {
    var services = new ServiceCollection();
    serviceCollectionConfiguration(services);

    foreach (var service in services) {
      providers.Add(new ProviderDescriptor {
        Token = service.ServiceType,
        ImplementationFactory = MakeServiceDescriptorFactory(service),
        Lifetime = ConvertServiceLifetime(service.Lifetime)
      });
    }

    return providers;
  }

  private static Func<IInjector, object> MakeServiceDescriptorFactory(ServiceDescriptor descriptor) {
    if (descriptor.ImplementationInstance != null) {
      return _ => descriptor.ImplementationInstance;
    }

    throw new InvalidOperationException("Invalid service descriptor");
  }

  private static InjectorLifetime ConvertServiceLifetime(ServiceLifetime lifetime) {
    return lifetime switch {
      ServiceLifetime.Singleton => InjectorLifetime.Singleton,
      ServiceLifetime.Scoped => InjectorLifetime.Scoped,
      ServiceLifetime.Transient => InjectorLifetime.Transient,
      _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null)
    };
  }
}
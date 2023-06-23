using Microsoft.Extensions.DependencyInjection;
using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.ServiceCollectionInterOpt.Provider;

public static class ProviderServiceCollectionExtensions {
  public static ProviderCollection AddFromServiceCollection(
    this ProviderCollection providers,
    Action<IServiceCollection> serviceCollectionConfiguration
  ) {
    IServiceCollection services;

    if (providers.TryGet(typeof(IServiceCollection), out var serviceCollectionProvider)) {
      services = (IServiceCollection)serviceCollectionProvider!.ImplementationInstance!;
    }
    else {
      services = new ServiceCollection();
      providers.Add(new ProviderDescriptor {
        Token = typeof(IServiceCollection),
        ImplementationInstance = services,
        Lifetime = InjectorLifetime.Singleton
      });

      providers.Add(new ProviderDescriptor {
        Token = typeof(IServiceProvider),
        ImplementationFactory = injector => new InjectorServiceProvider(injector),
        Lifetime = InjectorLifetime.Singleton
      });
    }

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
    if (descriptor.ImplementationInstance is not null) {
      return _ => descriptor.ImplementationInstance;
    }

    if (descriptor.ImplementationType is not null) {
      return injector => InjectorTypeActivator.CreateInstance(descriptor.ImplementationType, injector);
    }

    if (descriptor.ImplementationFactory is not null) {
      return injector => descriptor.ImplementationFactory(injector.Get<IServiceProvider>());
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
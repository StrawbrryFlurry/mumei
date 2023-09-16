using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Dynamic.Registration;
using Mumei.DependencyInjection.Providers.Registration;
using Mumei.DependencyInjection.ServiceCollectionInterOpt.Provider;

namespace CleanArchitectureApplication.Application;

public sealed class MediatrBinder<TAssemblyMarker> : IProviderBinder {
  public void Bind(ProviderCollection providers) {
    providers.AddFromServiceCollection(services => services.AddMediatR(c => {
      c.RegisterServicesFromAssemblyContaining<TAssemblyMarker>();
    }));
  }
}
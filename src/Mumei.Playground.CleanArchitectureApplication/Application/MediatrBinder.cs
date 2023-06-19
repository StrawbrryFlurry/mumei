using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.ServiceCollectionInterOpt.Provider;

namespace CleanArchitectureApplication.Application;

public sealed class MediatrBinder<TAssemblyMarker> : IProviderBinder {
  public static void Bind(ProviderCollection providers) {
    providers.AddFromServiceCollection(services => services.AddMediatR(c => {
      c.RegisterServicesFromAssemblyContaining<TAssemblyMarker>();
    }));
  }
}
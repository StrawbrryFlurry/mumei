using Mumei.AspNetCore.Common.Application;
using Mumei.DependencyInjection.Injector;

namespace Mumei.AspNetCore.Application;

internal sealed class MumeiWebApplicationBuilder : IMumeiWebApplicationBuilder {
  private readonly WebApplicationBuilder _applicationBuilder;
  private readonly ServiceCollectionAdapter _serviceCollection;

  public MumeiWebApplicationBuilder(WebApplicationBuilder applicationBuilder, IInjector environment) {
    _applicationBuilder = applicationBuilder;
    Injector = environment;

    _serviceCollection = new ServiceCollectionAdapter(environment);
  }

  public IServiceCollection Services => _serviceCollection;

  public IInjector Injector { get; }

  public IMumeiWebApplication Build() {
    var serviceProvider = BuildServiceProvider();
    return new MumeiWebApplication(_applicationBuilder.Build());
  }

  private IServiceProvider BuildServiceProvider() {
    return new ServiceProviderAdapter(_serviceCollection, Injector);
  }
}
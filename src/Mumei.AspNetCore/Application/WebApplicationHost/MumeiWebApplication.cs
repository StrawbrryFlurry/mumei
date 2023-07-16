using Microsoft.AspNetCore.Http.Features;
using Mumei.AspNetCore.Common.Application;
using Mumei.AspNetCore.Example.Generated;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Module;

namespace Mumei.AspNetCore.Application;

public sealed class MumeiWebApplication : IMumeiWebApplication {
  private readonly WebApplication _application;

  internal MumeiWebApplication(WebApplication application) {
    _application = application;
  }

  public void Dispose() {
    throw new NotImplementedException();
  }

  public Task StartAsync(CancellationToken cancellationToken = new()) {
    throw new NotImplementedException();
  }

  public Task StopAsync(CancellationToken cancellationToken = new()) {
    throw new NotImplementedException();
  }

  public IServiceProvider Services { get; }

  public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) {
    throw new NotImplementedException();
  }

  public IApplicationBuilder New() {
    throw new NotImplementedException();
  }

  public RequestDelegate Build() {
    throw new NotImplementedException();
  }

  public IServiceProvider ApplicationServices { get; set; }
  public IFeatureCollection ServerFeatures { get; }
  public IDictionary<string, object?> Properties { get; }

  public IApplicationBuilder CreateApplicationBuilder() {
    throw new NotImplementedException();
  }

  public IServiceProvider ServiceProvider { get; }
  public ICollection<EndpointDataSource> DataSources { get; }

  public ValueTask DisposeAsync() {
    throw new NotImplementedException();
  }

  public IConfiguration Configuration { get; }

  public static IMumeiWebApplicationBuilder CreateBuilder<TAppModule>() where TAppModule : IModule {
    var injector = PlatformInjector.CreateEnvironment<TAppModule>();
    var applicationBuilder = WebApplication.CreateBuilder();

    var builder = new MumeiWebApplicationBuilder(applicationBuilder, injector);
    return builder;
  }

  public static IMumeiWebApplicationBuilder CreateBuilder<TAppModule>(
    WebApplicationBuilder applicationBuilder
  ) where TAppModule : IModule, new() {
    var injector = PlatformInjector.CreateEnvironment<TAppModule>();

    var builder = new MumeiWebApplicationBuilder(applicationBuilder, injector);
    return builder;
  }

  public static IMumeiWebApplicationBuilder CreateBuilder<TAppModule>(
    WebApplicationBuilder applicationBuilder,
    IInjector environmentInjector
  ) where TAppModule : new() {
    var builder = new MumeiWebApplicationBuilder(applicationBuilder, environmentInjector);
    return builder;
  }
}
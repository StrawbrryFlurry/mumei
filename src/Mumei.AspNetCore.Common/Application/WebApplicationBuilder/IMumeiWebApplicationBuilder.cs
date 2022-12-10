using Mumei.DependencyInjection.Core;

namespace Mumei.AspNetCore.Common.Application;

public interface IMumeiWebApplicationBuilder {
  internal IInjector Injector { get; }
  public IServiceCollection Services { get; }
  public IMumeiWebApplication Build();
}
namespace Mumei.AspNetCore.Common.Application;

public interface IMumeiWebApplication : IHost, IApplicationBuilder, IEndpointRouteBuilder, IAsyncDisposable {
  public IConfiguration Configuration { get; }
}
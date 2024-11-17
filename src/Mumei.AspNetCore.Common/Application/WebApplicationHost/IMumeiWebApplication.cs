using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Mumei.AspNetCore.Common.Application;

public interface IMumeiWebApplication : IHost, IApplicationBuilder, IEndpointRouteBuilder, IAsyncDisposable {
    public IConfiguration Configuration { get; }
}
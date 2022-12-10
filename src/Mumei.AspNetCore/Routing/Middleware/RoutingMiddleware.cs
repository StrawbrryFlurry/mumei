namespace Mumei.AspNetCore.Routing;

public class RoutingMiddleware : IMiddleware {
  public Task InvokeAsync(HttpContext context, RequestDelegate next) {
    throw new NotImplementedException();
  }
}
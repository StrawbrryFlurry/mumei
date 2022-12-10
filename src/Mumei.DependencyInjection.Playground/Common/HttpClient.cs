namespace Mumei.DependencyInjection.Playground.Common;

public class HttpClient : IHttpClient {
  public Uri BaseAddress { get; set; } = null!;
}
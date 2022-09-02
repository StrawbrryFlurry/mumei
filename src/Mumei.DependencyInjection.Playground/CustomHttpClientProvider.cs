using Mumei.Core;
using Mumei.DependencyInjection.Playground.Common;
using HttpClient = Mumei.DependencyInjection.Playground.Common.HttpClient;

namespace Mumei.DependencyInjection.Playground;

public class CustomHttpClientProvider : IDynamicProvider<IHttpClient> {
  public IHttpClient Provide() {
    return new HttpClient();
  }
}
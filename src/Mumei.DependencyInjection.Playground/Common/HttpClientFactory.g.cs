using Mumei.Core;
using Mumei.DependencyInjection.Playground.Common;
using HttpClient = Mumei.DependencyInjection.Playground.Common.HttpClient;

namespace Mumei.DependencyInjection.Playground;

public class HttpClientλFactory : ScopedBindingFactory<IHttpClient> {
  protected override IHttpClient Create(IInjector scope) {
    return new HttpClient();
  }
}
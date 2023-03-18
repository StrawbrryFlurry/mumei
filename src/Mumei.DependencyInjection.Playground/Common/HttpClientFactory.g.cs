using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Playground.Common;
using HttpClient = Mumei.DependencyInjection.Playground.Common.HttpClient;

namespace Mumei.DependencyInjection.Playground;

public class HttpClientλBinding : ScopedBinding<IHttpClient> {
  protected override IHttpClient Create(IInjector scope) {
    return new HttpClient();
  }
}
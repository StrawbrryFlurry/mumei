using Mumei.DependencyInjection.Core;
using Mumei.Playground.SimpleApplication.Common;

namespace Mumei.DependencyInjection.Playground;

public class HttpClientλBinding : ScopedBinding<HttpClient> {
  protected override HttpClient Create(IInjector scope) {
    return new HttpClient();
  }
}
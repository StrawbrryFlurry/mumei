using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Resolution;
using Mumei.Playground.SimpleApplication.Common;

namespace Mumei.DependencyInjection.Playground;

public class λHttpClientBinding : ScopedBinding<HttpClient> {
  protected override HttpClient Create(IInjector scope) {
    return new HttpClient();
  }
}
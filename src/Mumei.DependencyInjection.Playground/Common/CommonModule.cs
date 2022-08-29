using Mumei.Core;
using Mumei.Core.Attributes;
using Mumei.DependencyInjection.Playground.Weather.generated;

namespace Mumei.DependencyInjection.Playground.Common;

public class CommonModule : IModule {
  private readonly Binding<IHttpClient> _httpClientBinding;

  public CommonModule() {
    _httpClientBinding = new ScopedBinding<IHttpClient>(new HttpClientλFactory());
  }

  public IHttpClient HttpClient => _httpClientBinding.Get();

  public ComponentRef<object>[] Components { get; }

  public T Get<T>() {
    throw new NotImplementedException();
  }

  public object Get(Type provider) {
    throw new NotImplementedException();
  }

  [Scoped<IHttpClient, HttpClient>]
  public void Providers() { }
}
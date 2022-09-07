using Mumei.Core;
using Mumei.Core.Attributes;
using Mumei.Core.Injector;
using Mumei.Core.Provider;

namespace Mumei.DependencyInjection.Playground.Common;

public sealed class CommonModule : IModule {
  public CommonModule() {
    HttpClientBinding = new HttpClientλFactory();
  }

  public IHttpClient HttpClient => HttpClientBinding.Get(this);

  public Binding<IHttpClient> HttpClientBinding { get; }

  public IInjector Parent { get; } = NullInjector.Instance;

  public T Get<T>(InjectFlags flags = InjectFlags.None) {
    var provider = typeof(T);
    var instance = provider switch {
      _ when provider == typeof(IHttpClient) => HttpClientBinding.Get(this),
      _ => Parent.Get(provider)
    };

    return (T)instance;
  }

  public object Get(object providerToken, InjectFlags flags = InjectFlags.None) {
    return providerToken switch {
      _ when providerToken == typeof(IHttpClient) => HttpClientBinding.Get(this),
      _ => Parent.Get(providerToken)
    };
  }

  public IInjector CreateScope() {
    throw new NotSupportedException("Cannot create scope of non user modules");
  }

  [Scoped<IHttpClient, HttpClient>]
  public void Providers() { }
}
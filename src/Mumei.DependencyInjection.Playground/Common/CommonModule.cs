﻿using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;

namespace Mumei.DependencyInjection.Playground.Common;

public sealed class CommonModule : IModule {
  internal readonly Binding<IHttpClient> HttpClientBinding;

  public CommonModule() {
    HttpClientBinding = new HttpClientλBinding();
  }

  public IHttpClient HttpClient => HttpClientBinding.Get(this);

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
      Type t when t == typeof(IHttpClient) => HttpClientBinding.Get(this),
      _ => Parent.Get(providerToken)
    };
  }

  public IInjector CreateScope() {
    throw new NotSupportedException("Cannot create scope of non user modules");
  }

  public IInjector CreateScope(IInjector context) {
    throw new NotImplementedException();
  }

  [Scoped<IHttpClient, HttpClient>]
  public void Providers() { }
}
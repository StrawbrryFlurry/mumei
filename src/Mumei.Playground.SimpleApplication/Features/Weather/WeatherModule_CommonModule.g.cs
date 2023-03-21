﻿using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Playground;
using WeatherApplication.Common;

namespace Mumei.Playground.SimpleApplication.Common; 

public sealed class λWeatherModuleλCommonModule : CommonModule {
  public override IInjector Parent { get; }
  
  internal readonly Binding<HttpClient> HttpClientBinding;

  public override HttpClient HttpClient => HttpClientBinding.Get();
  
  public λWeatherModuleλCommonModule() {
    HttpClientBinding = new HttpClientλBinding();
  }
  
  internal ScopedBinding<ILogger<TCategory>> MakeDynamicILoggerλ1<TCategory>() {
    return new ScopedGenericILoggerλ1Binding<TCategory>(this);
  }
  
  public override T Get<T>(InjectFlags flags = InjectFlags.None) {
    var provider = typeof(T);
    var instance = provider switch {
      _ when provider == typeof(HttpClient) => HttpClientBinding.Get(),
      _ => Parent.Get(provider)
    };

    return (T)instance;
  }

  public override object Get(object providerToken, InjectFlags flags = InjectFlags.None) {
    return providerToken switch {
      Type t when t == typeof(HttpClient) => HttpClientBinding.Get(this),
      _ => Parent.Get(providerToken)
    };
  }

  public override IInjector CreateScope() {
    throw new NotSupportedException(
      "Cannot create scope of CommonModule because it is marked as [CannotBeScoped]."
      );
  }

  public override IInjector CreateScope(IInjector context) {
    throw new NotSupportedException(
      "Cannot create scope of CommonModule because it is marked as [CannotBeScoped]."
    );
  }
}

file class ScopedGenericILoggerλ1Binding<TCategory> : ScopedBinding<ILogger<TCategory>> {
  private readonly λWeatherModuleλCommonModule _module;

  public ScopedGenericILoggerλ1Binding(λWeatherModuleλCommonModule module) {
    _module = module;
  }
  
  protected override ILogger<TCategory> Create(IInjector scope = null) {
    return _module.CreateLogger<TCategory>();
  }
}
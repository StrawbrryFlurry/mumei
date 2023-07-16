using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;
using Mumei.DependencyInjection.Playground;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Resolution;
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
  
  public override T Get<T>(IInjector? scope, InjectFlags flags = InjectFlags.None) {
    var provider = typeof(T);
    var instance = provider switch {
      _ when provider == typeof(HttpClient) => HttpClientBinding.Get(),
      _ => Parent.Get(provider)
    };

    return (T)instance;
  }

  public override object Get(object providerToken, IInjector? scope, InjectFlags flags = InjectFlags.None) {
    return providerToken switch {
      Type t when t == typeof(HttpClient) => HttpClientBinding.Get(this),
      _ => Parent.Get(providerToken)
    };
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

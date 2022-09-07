using System.Linq.Expressions;
using System.Reflection;
using Mumei.Core;
using Mumei.Core.Provider;
using Mumei.DependencyInjection.Playground;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Modules;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;

public sealed class WeatherModule : IModule, IWeatherModule {
  private static readonly MethodInfo ConfigureHttpClientMethodInfo = typeof(IWeatherModule)
    .GetMethod(
      nameof(IWeatherModule.ConfigureHttpClient),
      BindingFlags.Instance | BindingFlags.Public
      )!;

  internal readonly Binding<IHttpClient> HttpClientBinding;
  internal readonly Binding<WeatherController> WeatherControllerBinding;
  internal readonly Binding<IWeatherService> WeatherServiceBinding;

  public IInjector Parent { get; }

  public WeatherModule(IInjector parent, CommonModule commonModule) {
    Parent = parent;
    HttpClientBinding = commonModule.HttpClientBinding;

    // Works with existing bindings
    // Works with configuration methods
    // Maybe works with inject flags / Injector specific configuration
    WeatherServiceBinding = new IWeatherServiceλBindingFactory(
      new ApplyIWeatherServiceIHttpClientConfiguration(
        ConfigureHttpClientMethodInfo,
        this,
        commonModule.HttpClientBinding
      )
    );
    
    WeatherControllerBinding = new WeatherControllerλBindingFactory(WeatherServiceBinding);
  }
  
  public T Get<T>(InjectFlags flags= InjectFlags.None) {
    if (flags.HasFlag(InjectFlags.SkipHost)) {
      return Parent.Get<T>();
    }
    
    var provider = typeof(T);
    var instance = provider switch {
      _ when provider == typeof(IWeatherService) => WeatherServiceBinding.Get(this),
      _ when provider == typeof(WeatherController) => WeatherControllerBinding.Get(this),
      _ when provider == typeof(IHttpClient) => HttpClientBinding.Get(this),
      _ => Parent.Get(provider)
    };

    return (T)instance;
  }

  public object Get(object token, InjectFlags flags= InjectFlags.None) {
    return token switch {
      _ when token == typeof(IWeatherService) => WeatherServiceBinding.Get(this),
      _ when token == typeof(WeatherController) => WeatherControllerBinding.Get(this),
      _ when token == typeof(IHttpClient) => HttpClientBinding.Get(this),
      _ => Parent.Get(token)
    };
  }

  public IInjector CreateScope() {
    return new WeatherModuleλScopeInjector(Parent, this);
  }

  public IWeatherService WeatherService => WeatherServiceBinding.Get();

  private sealed class WeatherModuleλScopeInjector : IInjector {
    private readonly WeatherModule _module;

    public WeatherModuleλScopeInjector(IInjector parent, WeatherModule module) {
      Parent = parent;
      _module = module;
    }

    public IInjector Parent { get; }

    public T Get<T>(InjectFlags flags = InjectFlags.None) {
      return (T)Get(typeof(T));
    }

    public object Get(object token, InjectFlags flags = InjectFlags.None) {
      return token switch {
        _ when token == typeof(IWeatherService) => _module.WeatherServiceBinding.Get(this),
        _ when token == typeof(WeatherController) => _module.WeatherControllerBinding.Get(this),
        _ when token == typeof(IHttpClient) => _module.HttpClientBinding.Get(this),
        _ => Parent.Get(token)
      };
    }
  }
}

internal class ApplyIWeatherServiceIHttpClientConfiguration : ApplyBindingConfigurationFactory<IHttpClient> {
  private readonly MethodInfo _configureMethod;
  private readonly object _configureMethodTarget;
  private readonly Binding<IHttpClient> _httpClientProvider;

  public ApplyIWeatherServiceIHttpClientConfiguration(
    MethodInfo configureMethod,
    object configureMethodTarget,
    Binding<IHttpClient> httpClientProvider
  ) {
    _configureMethod = configureMethod;
    _configureMethodTarget = configureMethodTarget;
    _httpClientProvider = httpClientProvider;
  }
  
  public override IHttpClient Get(IInjector scope = null) {
    return ConfigureHttpClient(_httpClientProvider.Get());
  }
  
  private IHttpClient ConfigureHttpClient(IHttpClient httpClient) {
    return (IHttpClient)_configureMethod.Invoke(_configureMethodTarget, new object[] { httpClient })!;
  } 
}
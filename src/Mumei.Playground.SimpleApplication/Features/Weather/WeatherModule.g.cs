using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Internal;
using Mumei.DependencyInjection.Playground;
using Mumei.DependencyInjection.Playground.Example.Modules;
using Mumei.Playground.SimpleApplication.Common;
using WeatherApplication.Features.Weather;
using WeatherApplication.Features.Weather.Controllers;
using WeatherApplication.Features.Weather.Services;

[MumeiGenerated]
[MumeiModuleImplFor<WeatherModule>]
public sealed class λWeatherModule : WeatherModule {
  private static readonly MethodInfo ConfigureHttpClientMethodInfo = typeof(WeatherModule)
    .GetMethod(
      nameof(ConfigureHttpClient),
      BindingFlags.Instance | BindingFlags.Public
      )!;

  internal readonly λWeatherModuleλCommonModule λCommonModule;
  
  internal readonly Binding<WeatherController> WeatherControllerBinding;
  internal readonly Binding<IWeatherService> WeatherServiceBinding;
  internal readonly Binding<ILogger<WeatherService>> ILoggerλWeatherServiceBinding;

  public override λWeatherModuleλCommonModule CommonModule => λCommonModule;
  
  public override IWeatherService WeatherService => WeatherServiceBinding.Get();
  
  public override IInjector Parent { get; }

  public λWeatherModule(IInjector parent, λWeatherModuleλCommonModule commonModule) {
    Parent = parent;
    λCommonModule = commonModule;

    ILoggerλWeatherServiceBinding = λCommonModule.MakeDynamicILoggerλ1<WeatherService>();

    WeatherServiceBinding = new WeatherServiceλBinding(
      new ApplyIWeatherServiceHttpClientConfiguration(
        ConfigureHttpClientMethodInfo,
        this,
        commonModule.HttpClientBinding
      ),
      ILoggerλWeatherServiceBinding
    );

    WeatherControllerBinding = new WeatherControllerλBinding(WeatherServiceBinding);
  }
  
  public override T Get<T>(InjectFlags flags= InjectFlags.None) {
    if ((flags & InjectFlags.SkipHost) == 0) {
      return Parent.Get<T>();
    }

    var provider = typeof(T);
    if (λWeatherModuleInjectableBloom.Contains(provider)) {
      // Contains all providers from this module and recursively all providers from imported modules.
      // In cases where an import is a dynamic module, HasProvider is called to check if the provider exists on the module.
      // Duplicate providers are omitted, the first one found is used.
      var instance = provider switch {
        _ when provider == typeof(IWeatherService) => WeatherServiceBinding.Get(this),
        _ when provider == typeof(ILogger<WeatherService>) => ILoggerλWeatherServiceBinding.Get(this),
        _ when provider == typeof(WeatherController) => WeatherControllerBinding.Get(this),
        // Only store a local decorated binding from an import if there is a global configuration for that
        // provider in the module. Consumers could then use the type provided from that module to get 
        // the configured instance.
        _ when provider == typeof(HttpClient) => CommonModule.HttpClientBinding.Get(this),
        _ => Parent.Get(provider)
      };

      return (T)instance; 
    }
    
    if((flags & InjectFlags.Host) != 0) {
      return Parent.Get<T>();
    }
    
    throw NullInjector.Exception(provider);
  }

  public override object Get(object token, InjectFlags flags= InjectFlags.None) {
    return token switch {
      _ when token == typeof(IWeatherService) => WeatherServiceBinding.Get(this),
      _ when token == typeof(ILogger<WeatherService>) => ILoggerλWeatherServiceBinding.Get(this),
      _ when token == typeof(WeatherController) => WeatherControllerBinding.Get(this),
      _ when token == typeof(HttpClient) => CommonModule.HttpClientBinding.Get(this),
      _ => Parent.Get(token)
    };
  }

  public override IInjector CreateScope() {
    return new WeatherModuleλScopeInjector(Parent, this);
  }

  public override IInjector CreateScope(IInjector ctx) {
    var decoratedParent = new DecoratedInjector(ctx, this);
    return new WeatherModuleλScopeInjector(decoratedParent, this);
  }

  private sealed class WeatherModuleλScopeInjector : IInjector {
    private readonly λWeatherModule _module;

    public WeatherModuleλScopeInjector(IInjector parent, λWeatherModule module) {
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
        _ when token == typeof(ILogger<WeatherService>) => _module.ILoggerλWeatherServiceBinding.Get(this),
        _ when token == typeof(WeatherController) => _module.WeatherControllerBinding.Get(this),
        _ when token == typeof(HttpClient) => _module.CommonModule.HttpClientBinding.Get(this),
        _ => Parent.Get(token)
      };
    }
  }
  
  // This can be static because we already know from the module definition what
  // injectables are present in the module.
  private static class λWeatherModuleInjectableBloom {
    // This will already be initialized when the module is created
    private static readonly long[] _providerBloomVectors = {
      0b_000000000000000000000000000000,
      0b_000000000000000000000000000000,
      0b_000000000000000000000000000000,
      0b_000000000000000000000000000000,
    };

    private const int _vectorCount = 4;
    private const int _bloomSize = 64 * _vectorCount; // 64 bits (long) per vector * 4 vectors
    private const int _bloomMask = _bloomSize - 1;

    internal static bool Contains(Type token) {
      return Contains(token.GUID.GetHashCode());
    }

    internal static bool Contains(string token) {
      return Contains(token.GetHashCode());
    } 
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Contains(object token) {
      return Contains(token.GetHashCode());
    }
    
    private static bool Contains(int hash) {
      hash = ~hash + 1; // Ensure that the hash is positive
      var mask = 1l << hash;
      var vectorIdx = ~~(hash / 64l);
      var bucketVector = _providerBloomVectors[vectorIdx];

      return (mask & bucketVector) != 0u;
    }

      // Example of what the generator will do
    private static void Add(int hash) {
      var mask = 1 << hash;
      var buckedIdx = (hash & _vectorCount) + 1;
      _providerBloomVectors[buckedIdx] |= mask;
    }
  }
}

file class ApplyIWeatherServiceHttpClientConfiguration : ApplyBindingConfigurationFactory<HttpClient> {
  private readonly MethodInfo _configureMethod;
  private readonly object _configureMethodTarget;
  private readonly Binding<HttpClient> _httpClientProvider;

  public ApplyIWeatherServiceHttpClientConfiguration(
    MethodInfo configureMethod,
    object configureMethodTarget,
    Binding<HttpClient> httpClientProvider
  ) {
    _configureMethod = configureMethod;
    _configureMethodTarget = configureMethodTarget;
    _httpClientProvider = httpClientProvider;
  }
  
  public override HttpClient Get(IInjector scope = null) {
    return ConfigureHttpClient(_httpClientProvider.Get());
  }
  
  private HttpClient ConfigureHttpClient(HttpClient httpClient) {
    return (HttpClient)_configureMethod.Invoke(_configureMethodTarget, new object[] { httpClient })!;
  } 
}
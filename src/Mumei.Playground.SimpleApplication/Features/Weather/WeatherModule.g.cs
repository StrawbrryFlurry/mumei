using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;
using Mumei.DependencyInjection.Injector.Implementation;
using Mumei.DependencyInjection.Module.CodeGen;
using Mumei.DependencyInjection.Playground;
using Mumei.DependencyInjection.Playground.Example.Modules;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Resolution;
using Mumei.Playground.SimpleApplication.Common;
using WeatherApplication.Features.Weather;
using WeatherApplication.Features.Weather.Controllers;
using WeatherApplication.Features.Weather.Services;

[CompilerGenerated]
[MumeiModuleImplFor<WeatherModule>]
public sealed class λWeatherModule : WeatherModule {
  public override IInjector Parent { get; }

  private static readonly MethodInfo ConfigureHttpClientMethodInfo = typeof(WeatherModule)
    .GetMethod(
      nameof(ConfigureHttpClient),
      BindingFlags.Instance | BindingFlags.Public
    )!;

  internal readonly λWeatherModuleλCommonModule _commonModule;

  internal readonly Binding<WeatherController> WeatherControllerBinding;
  internal readonly Binding<IWeatherService> WeatherServiceBinding;
  internal readonly Binding<ILogger<WeatherService>> ILoggerλWeatherServiceBinding;

  public override λWeatherModuleλCommonModule CommonModule => _commonModule;
  public override IWeatherService WeatherService => WeatherServiceBinding.Get();

  public λWeatherModule(IInjector parent, λWeatherModuleλCommonModule commonModule) {
    Parent = parent;
    _commonModule = commonModule;

    ILoggerλWeatherServiceBinding = _commonModule.MakeDynamicILoggerλ1<WeatherService>();

    WeatherServiceBinding = new λWeatherServiceBinding(
      new ApplyIWeatherServiceHttpClientConfiguration(
        ConfigureHttpClientMethodInfo,
        this,
        commonModule.HttpClientBinding
      ),
      ILoggerλWeatherServiceBinding
    );

    WeatherControllerBinding = new λWeatherControllerBinding(WeatherServiceBinding);
  }

  public override TProvider Get<TProvider>(IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    return (TProvider)Get(typeof(TProvider), scope, flags);
  }

  public override object Get(object token, IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    scope ??= this;
    if ((flags & InjectFlags.SkipSelf) != 0) {
      return Parent.Get(token, scope, flags & ~InjectFlags.SkipSelf);
    }

    // Contains all providers from this module and recursively all providers from imported modules.
    // In cases where an import is a dynamic module, HasProvider is called to check if the provider exists on the module.
    // Duplicate providers are omitted, the first one found is used.
    object instance = token switch {
      _ when token == typeof(IWeatherService) => WeatherServiceBinding.Get(scope),
      _ when token == typeof(ILogger<WeatherService>) => ILoggerλWeatherServiceBinding.Get(scope),
      _ when token == typeof(WeatherController) => WeatherControllerBinding.Get(scope),
      // Only store a local decorated binding from an import if there is a global configuration for that
      // provider in the module. Consumers could then use the type provided from that module to get 
      // the configured instance.
      _ when token == typeof(HttpClient) => CommonModule.HttpClientBinding.Get(scope),
    };

    var isOptional = (flags & InjectFlags.Optional) != 0;
    var resolveInSelf = (flags & InjectFlags.Self) != 0;
    if (resolveInSelf && isOptional) {
      return default;
    }

    if (resolveInSelf) {
      NullInjector.Throw(token);
    }

    if ((flags & InjectFlags.Host) != 0) {
      return Parent.Get(token, scope, flags & ~InjectFlags.Host);
    }

    if (isOptional) {
      return default;
    }

    NullInjector.Throw(token);
    return default;
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
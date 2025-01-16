using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;
using Mumei.DependencyInjection.Injector.Implementation;
using Mumei.Playground.SimpleApplication.Common;
using WeatherApplication;
using WeatherApplication.Common;
using WeatherApplication.Features.Weather;
using WeatherApplication.Generated;

namespace Mumei.Playground.SimpleApplication; 

public sealed class λApplicationModule : IApplicationModule {
  internal readonly CommonModule _commonModule;
  internal readonly λWeatherModule _weatherModule;
  
  public WeatherModule WeatherModule => _weatherModule;
  public CommonModule CommonModule => _commonModule;
  
  public IInjector Scope { get; } = SingletonScope.Instance;
  public IInjector Parent { get; } = PlatformInjector.Instance;
  
  public TProvider Get<TProvider>(IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public object Get(object token, IInjector scope = null, InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public λApplicationModule(λWeatherModule weatherModule, CommonModule commonModule) {
    _weatherModule = weatherModule;
    _commonModule = commonModule;
  }
}
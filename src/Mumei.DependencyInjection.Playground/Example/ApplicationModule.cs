using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Internal;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Generated;
using Mumei.DependencyInjection.Playground.Example.Modules;

namespace Mumei.DependencyInjection.Playground.Example;

[Module]
[ApplicationRoot]
[Import<WeatherModule>]
[Import<CommonModule>]
public partial interface IApplicationModule { }

[MumeiGenerated]
public partial interface IApplicationModule : IModule {
  public WeatherModule WeatherModule { get; }
  public CommonModule CommonModule { get; }
}

[MumeiGenerated]
[MumeiModuleOf<IApplicationModule>]
public sealed class ApplicationModuleImpl : IApplicationModule {
  internal readonly CommonModule λCommonModule;
  internal readonly λMumeiλWeatherModule λWeatherModule;

  public ApplicationModuleImpl(λMumeiλWeatherModule weatherModule, CommonModule commonModule) {
    λWeatherModule = weatherModule;
    λCommonModule = commonModule;
  }

  public IInjector Parent { get; } = PlatformInjector.Instance;

  public TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public object Get(object token, InjectFlags flags = InjectFlags.None) {
    throw new NotImplementedException();
  }

  public IInjector CreateScope() {
    throw new NotImplementedException();
  }

  public IInjector CreateScope(IInjector context) {
    throw new NotImplementedException();
  }

  public WeatherModule WeatherModule => λWeatherModule;
  public CommonModule CommonModule => λCommonModule;
}
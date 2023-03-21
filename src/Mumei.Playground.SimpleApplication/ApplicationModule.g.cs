﻿using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Internal;
using Mumei.Playground.SimpleApplication.Common;
using WeatherApplication;
using WeatherApplication.Common;
using WeatherApplication.Features.Weather;
using WeatherApplication.Generated;

namespace Mumei.Playground.SimpleApplication; 

[MumeiGenerated]
[MumeiModuleImplFor<IApplicationModule>]
public sealed class λApplicationModule : IApplicationModule {
  internal readonly CommonModule λCommonModule;
  internal readonly λWeatherModule λWeatherModule;
  
  public WeatherModule WeatherModule => λWeatherModule;
  public CommonModule CommonModule => λCommonModule;
  
  public IInjector Parent { get; } = PlatformInjector.Instance;

  public λApplicationModule(λWeatherModule weatherModule, CommonModule commonModule) {
    λWeatherModule = weatherModule;
    λCommonModule = commonModule;
  }

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

}
﻿using Mumei.Core;
using Mumei.Core.Attributes;
using Mumei.Core.Provider;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;
using Mumei.DependencyInjection.Playground.Generated;
using Mumei.Internal;

namespace Mumei.DependencyInjection.Playground.Example;

[Module]
[ApplicationRoot]
[Import<WeatherModule>]
[Import<CommonModule>]
public interface IApplicationModule { }

[MumeiModule]
public sealed class ApplicationModule : IApplicationModule, IModule {
  internal readonly Binding<IHttpClient> HttpClientBinding;
  internal readonly WeatherModule WeatherModule;
  internal readonly Binding<IWeatherService> WeatherServiceBinding;

  public ApplicationModule(WeatherModule weatherModule, CommonModule commonModule) {
    WeatherModule = weatherModule;
    HttpClientBinding = commonModule.HttpClientBinding;
    WeatherServiceBinding = weatherModule.WeatherServiceBinding;
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
}
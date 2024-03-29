﻿using System.Runtime.CompilerServices;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Module.Registration;
using WeatherApplication.Common;
using WeatherApplication.Features.Weather;

namespace WeatherApplication;

[RootModule]
[Import<WeatherModule>]
[Import<CommonModule>]
public partial interface IApplicationModule { }

[CompilerGenerated]
public partial interface IApplicationModule : IGlobalModule {
  public WeatherModule WeatherModule { get; }
  public CommonModule CommonModule { get; }
}
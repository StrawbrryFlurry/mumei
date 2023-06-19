using System.Runtime.CompilerServices;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;
using WeatherApplication.Common;
using WeatherApplication.Features.Weather;

namespace WeatherApplication;

[Entrypoint]
[Import<WeatherModule>]
[Import<CommonModule>]
public partial interface IApplicationModule { }

[CompilerGenerated]
public partial interface IApplicationModule : IGlobalModule {
  public WeatherModule WeatherModule { get; }
  public CommonModule CommonModule { get; }
}
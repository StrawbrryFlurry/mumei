using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Internal;
using WeatherApplication.Common;
using WeatherApplication.Features.Weather;

namespace WeatherApplication;

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
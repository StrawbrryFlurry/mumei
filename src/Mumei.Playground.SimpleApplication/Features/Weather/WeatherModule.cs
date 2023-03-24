using System.Runtime.CompilerServices;
using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;
using WeatherApplication.Common;
using WeatherApplication.Features.Weather.Controllers;
using WeatherApplication.Features.Weather.Services;

namespace WeatherApplication.Features.Weather;

[Module]
[Import<CommonModule>]
[Component<WeatherController>]
public partial class WeatherModule {
  [Transient<WeatherService>]
  public abstract IWeatherService WeatherService { get; }

  [ConfigureFor<IWeatherService>]
  protected HttpClient ConfigureHttpClient(HttpClient httpClient) {
    httpClient.BaseAddress = new Uri("https://api.openweathermap.org");
    return httpClient;
  }
}

[CompilerGenerated]
public abstract partial class WeatherModule : IModule {
  public abstract CommonModule CommonModule { get; }

  public abstract IInjector Parent { get; }
  public abstract TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, InjectFlags flags = InjectFlags.None);
  public abstract IInjector CreateScope();
  public abstract IInjector CreateScope(IInjector context);
}
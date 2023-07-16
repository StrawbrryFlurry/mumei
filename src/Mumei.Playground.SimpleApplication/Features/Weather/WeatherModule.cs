using System.Runtime.CompilerServices;
using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Injector.Behavior;
using Mumei.DependencyInjection.Module;
using Mumei.DependencyInjection.Module.Markers;
using Mumei.DependencyInjection.Module.Registration;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Registration;
using WeatherApplication.Common;
using WeatherApplication.Features.Weather.Controllers;
using WeatherApplication.Features.Weather.Services;

namespace WeatherApplication.Features.Weather;

[Module]
[Import<CommonModule>]
[Scoped<WeatherController>]
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

  public abstract TProvider Get<TProvider>(IInjector? scope = null, InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, IInjector? scope = null, InjectFlags flags = InjectFlags.None);
}
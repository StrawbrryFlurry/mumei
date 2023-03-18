using Mumei.DependencyInjection.Attributes;
using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Internal;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;

namespace Mumei.DependencyInjection.Playground.Example.Modules;

[Module]
[Import<CommonModule>]
[Component<WeatherController>]
// We might want to support interfaces as well.
public partial class WeatherModule {
  [Transient<WeatherService>]
  public abstract IWeatherService WeatherService { get; }

  [ConfigureFor<IWeatherService>]
  public IHttpClient ConfigureHttpClient(IHttpClient httpClient) {
    httpClient.BaseAddress = new Uri("https://api.openweathermap.org");
    return httpClient;
  }

  [Singleton]
  [DynamicBinding]
  public IWeatherService SingletonWeatherService() {
    return new WeatherService(CommonModule.HttpClient);
  }
}

[MumeiGenerated]
public abstract partial class WeatherModule : IModule {
  public abstract CommonModule CommonModule { get; }
  public abstract IWeatherService SingletonWeatherServiceDynamic { get; }

  public abstract IInjector Parent { get; }
  public abstract TProvider Get<TProvider>(InjectFlags flags = InjectFlags.None);
  public abstract object Get(object token, InjectFlags flags = InjectFlags.None);
  public abstract IInjector CreateScope();
  public abstract IInjector CreateScope(IInjector context);
}
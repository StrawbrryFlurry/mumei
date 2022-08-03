using Mumei.Core;
using Mumei.Core.Attributes;
using Mumei.DependencyInjection.Playground.Weather;

namespace Mumei.DependencyInjection.Playground;

[Module]
public partial class AppModule {
  [Import<WeatherModule>]
  partial void Imports();
}

public partial class AppModule : IModule {
  public WeatherModule WeatherModule { get; }
  public IWeatherService Weather_WeatherService => WeatherModule.WeatherService;

  public T Get<T>() {
    throw new NotImplementedException();
  }

  public object Get(Type provider) {
    throw new NotImplementedException();
  }

  partial void Imports() { }
}
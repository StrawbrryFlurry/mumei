using Mumei.DependencyInjection.Core;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;

#nullable enable

namespace Mumei.DependencyInjection.Playground.Example.Modules; 

public class WeatherControllerλBinding : ScopedBinding<WeatherController> {
  private readonly Binding<IWeatherService> _weatherServiceProvider;

  public WeatherControllerλBinding(Binding<IWeatherService> weatherServiceProvider) {
    _weatherServiceProvider = weatherServiceProvider;
  }

  protected override WeatherController Create(IInjector? scope = null) {
    return new WeatherController(_weatherServiceProvider.Get(scope));
  }
}
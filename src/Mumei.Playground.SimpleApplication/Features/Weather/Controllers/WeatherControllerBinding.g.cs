using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Resolution;
using WeatherApplication.Features.Weather.Controllers;
using WeatherApplication.Features.Weather.Services;

namespace Mumei.DependencyInjection.Playground.Example.Modules; 

public class WeatherControllerλBinding : ScopedBinding<WeatherController> {
  private readonly Binding<IWeatherService> _weatherServiceProvider;

  public WeatherControllerλBinding(Binding<IWeatherService> weatherServiceProvider) {
    _weatherServiceProvider = weatherServiceProvider;
  }

  protected override WeatherController Create(IInjector scope = null) {
    return new WeatherController(_weatherServiceProvider.Get(scope));
  }
}
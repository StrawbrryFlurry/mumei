using Mumei.Core;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;

namespace Mumei.DependencyInjection.Playground.Example.Modules; 

public class WeatherControllerλFactory : IProviderFactory<WeatherController> {
  private readonly IProviderFactory<IWeatherService> _weatherServiceProvider;
  private readonly IProviderFactory<OptionalService> _optionalServiceProvider;

  public WeatherControllerλFactory(IProviderFactory<IWeatherService> weatherServiceProvider, IProviderFactory<OptionalService> optionalServiceProvider) {
    _weatherServiceProvider = weatherServiceProvider;
    _optionalServiceProvider = optionalServiceProvider;
  }
  
  public WeatherController Get() {
    return new WeatherController(_weatherServiceProvider.Get(), _optionalServiceProvider.Get());
  }
}
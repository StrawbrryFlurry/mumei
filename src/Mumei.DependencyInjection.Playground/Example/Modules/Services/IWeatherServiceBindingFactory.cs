using Mumei.Core;
using Mumei.Core.Provider;
using Mumei.DependencyInjection.Playground.Common;

namespace Mumei.DependencyInjection.Playground.Example.Modules.Services;

// ReSharper disable once InconsistentNaming
public class IWeatherServiceλBindingFactory : ScopedBindingFactory<IWeatherService> {
  private readonly Binding<IHttpClient> _httpClientBinding;

  public IWeatherServiceλBindingFactory(Binding<IHttpClient> httpClientBinding) {
    _httpClientBinding = httpClientBinding;
  }

  protected override IWeatherService Create(IInjector? scope = null) {
    return new WeatherService(_httpClientBinding.Get(scope));
  }
}
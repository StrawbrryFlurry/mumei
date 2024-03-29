﻿using Mumei.DependencyInjection.Injector;
using Mumei.DependencyInjection.Providers;
using Mumei.DependencyInjection.Providers.Resolution;

namespace WeatherApplication.Features.Weather.Services;

public sealed class λWeatherServiceBinding : ScopedBinding<IWeatherService> {
  private readonly Binding<HttpClient> _httpClientBinding;
  private readonly Binding<ILogger<WeatherService>> _loggerWeatherServiceBinding;

  public λWeatherServiceBinding(
    Binding<HttpClient> httpClientBinding,
    Binding<ILogger<WeatherService>> loggerWeatherServiceBinding
  ) {
    _httpClientBinding = httpClientBinding;
    _loggerWeatherServiceBinding = loggerWeatherServiceBinding;
  }

  protected override IWeatherService Create(IInjector? scope = null) {
    return new WeatherService(
      _loggerWeatherServiceBinding.Get(scope),
      _httpClientBinding.Get(scope)
    );
  }
}
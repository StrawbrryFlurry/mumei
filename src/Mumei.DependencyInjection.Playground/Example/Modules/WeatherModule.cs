using Mumei.Core;
using Mumei.Core.Attributes;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;
using Mumei.Internal;

namespace Mumei.DependencyInjection.Playground.Example.Modules;

[Module]
[Import<CommonModule>]
[Component<WeatherController>]
public interface IWeatherModule {
  [Singleton<WeatherService>]
  public IWeatherService WeatherService { get; }

  [ConfigureFor<IWeatherService>]
  public IHttpClient ConfigureHttpClient(IHttpClient httpClient) {
    httpClient.BaseAddress = new Uri("https://api.openweathermap.org");
    return httpClient;
  }
}

[MumeiModule]
public sealed class WeatherModule : IModule, IWeatherModule {
  internal readonly Binding<IHttpClient> HttpClientBinding;
  internal readonly Binding<WeatherController> WeatherControllerBinding;
  internal readonly Binding<IWeatherService> WeatherServiceBinding;

  public WeatherModule(IInjector parent, CommonModule commonModule) {
    Parent = parent;

    HttpClientBinding = commonModule.HttpClientBinding;
    WeatherServiceBinding = new SingletonBinding<IWeatherService>(new IWeatherServiceλFactory(null));
    WeatherControllerBinding =
      new ScopedBinding<WeatherController>(
        new WeatherControllerλFactory(
          new IWeatherServiceλFactory(
            new HttpClientλFactory()
          ),
          new OptionalServiceλFactory()
        )
      );
  }

  public IInjector Parent { get; }

  public T Get<T>() {
    var provider = typeof(T);
    var instance = provider switch {
      _ when provider == typeof(IWeatherService) => WeatherServiceBinding.Get(this),
      _ when provider == typeof(WeatherController) => WeatherControllerBinding.Get(this),
      _ => Parent.Get(provider)
    };

    return (T)instance;
  }

  public object Get(Type provider) {
    return provider switch {
      _ when provider == typeof(IWeatherService) => WeatherServiceBinding.Get(this),
      _ when provider == typeof(WeatherController) => WeatherControllerBinding.Get(this),
      _ => Parent.Get(provider)
    };
  }

  public IInjector CreateScope() {
    return new WeatherModuleλScopeInjector(Parent, this);
  }

  public IWeatherService WeatherService => WeatherServiceBinding.Get();

  private sealed class WeatherModuleλScopeInjector : IInjector {
    private readonly WeatherModule _module;

    public WeatherModuleλScopeInjector(IInjector parent, WeatherModule module) {
      Parent = parent;
      _module = module;
    }

    public IInjector Parent { get; }

    public T Get<T>() {
      return (T)Get(typeof(T));
    }

    public object Get(Type provider) {
      return provider switch {
        _ when provider == typeof(IWeatherService) => _module.WeatherServiceBinding.Get(this),
        _ when provider == typeof(WeatherController) => _module.WeatherControllerBinding.Get(this),
        _ => Parent.Get(provider)
      };
    }
  }
}
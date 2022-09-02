using Mumei.Core;
using Mumei.Core.Attributes;
using Mumei.DependencyInjection.Playground.Common;
using Mumei.DependencyInjection.Playground.Example.Modules;
using Mumei.DependencyInjection.Playground.Example.Modules.Services;
using Mumei.DependencyInjection.Playground.Generated;
using Mumei.Internal;

namespace Mumei.DependencyInjection.Playground.Example;

[Module]
[ApplicationRoot]
[Import<WeatherModule>]
[Import<CommonModule>]
public interface IApplicationModule { }

[MumeiModule]
public sealed class ApplicationModule : IApplicationModule, IModule {
  internal readonly Binding<IHttpClient> HttpClientBinding;
  internal readonly Binding<IWeatherService> WeatherServiceBinding;

  public ApplicationModule(WeatherModule weatherModule, CommonModule commonModule) {
    HttpClientBinding = commonModule.HttpClientBinding;
    WeatherServiceBinding = weatherModule.WeatherServiceBinding;
  }

  public IInjector Parent { get; } = PlatformInjector.Instance;

  public T Get<T>() {
    throw new NotImplementedException();
  }

  public object Get(Type provider) {
    throw new NotImplementedException();
  }

  public IInjector CreateScope() {
    throw new NotImplementedException();
  }
}